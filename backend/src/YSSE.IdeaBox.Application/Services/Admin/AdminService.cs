using Microsoft.EntityFrameworkCore;
using YSSE.IdeaBox.Application.Abstractions;
using YSSE.IdeaBox.Application.Abstractions.Services;
using YSSE.IdeaBox.Application.Common;
using YSSE.IdeaBox.Application.Dtos;
using YSSE.IdeaBox.Domain.Entities;
using YSSE.IdeaBox.Domain.Enums;

namespace YSSE.IdeaBox.Application.Services.Admin;

public sealed class AdminService : IAdminService
{
    private static readonly IdeaStatus[] PendingStatuses =
        { IdeaStatus.Submitted, IdeaStatus.UnderReview, IdeaStatus.RevisionRequired };

    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _current;
    private readonly IDateTimeProvider _clock;
    private readonly IAuditWriter _audit;

    public AdminService(
        IUnitOfWork db,
        ICurrentUserService current,
        IDateTimeProvider clock,
        IAuditWriter audit)
    {
        _unitOfWork = db;
        _current = current;
        _clock = clock;
        _audit = audit;
    }

    public async Task<PagedResult<AdminUserDto>> GetUsersAsync(AdminUserQuery query, CancellationToken ct = default)
    {
        RequireAdmin();

        var (page, pageSize) = Pagination.Normalize(query.Page, query.PageSize);
        var q = _unitOfWork.Repository<User>().Query().Where(u => !u.IsDeleted);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var pattern = $"%{query.Search.Trim().ToLower()}%";
            q = q.Where(u => EF.Functions.Like(u.DisplayName.ToLower(), pattern) ||
                             (u.Email != null && EF.Functions.Like(u.Email.ToLower(), pattern)));
        }

        if (!string.IsNullOrWhiteSpace(query.Role))
        {
            if (Enum.TryParse<RoleName>(query.Role, true, out var role) &&
                Enum.IsDefined(role) && role != 0)
            {
                q = q.Where(u => u.UserRoles.Any(ur => ur.Role.Name == role));
            }
            else
            {
                return PagedResult<AdminUserDto>.Create(new List<AdminUserDto>(), page, pageSize, 0);
            }
        }

        if (query.IsSuspended is not null)
        {
            q = q.Where(u => u.IsSuspended == query.IsSuspended.Value);
        }

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new AdminUserDto
            {
                Id = u.Id,
                DisplayName = u.DisplayName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                IsSuspended = u.IsSuspended,
                SuspensionReason = u.SuspensionReason,
                IsEmailVerified = u.IsEmailVerified,
                Roles = u.UserRoles.Select(ur => ur.Role.Name.ToString()).ToList(),
                CreatedAt = u.CreatedAt
            })
            .ToListAsync(ct);

        return PagedResult<AdminUserDto>.Create(items, page, pageSize, total);
    }

    public async Task<Result> SuspendUserAsync(int userId, SuspendUserRequest request, CancellationToken ct = default)
    {
        RequireAdmin();

        if (userId == _current.UserId)
        {
            return Result.Fail("You cannot suspend your own account.");
        }

        var user = await _unitOfWork.Repository<User>().Query().FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, ct);
        if (user is null)
        {
            throw new NotFoundException("User not found.");
        }

        user.IsSuspended = true;
        user.SuspensionReason = request.Reason.Trim();
        user.UpdatedAt = _clock.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        await _audit.RecordAsync("UserSuspended", nameof(User), user.Id.ToString(),
            request.Reason, null, ct);
        return Result.Ok();
    }

    public async Task<Result> ReactivateUserAsync(int userId, CancellationToken ct = default)
    {
        RequireAdmin();

        var user = await _unitOfWork.Repository<User>().Query().FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, ct);
        if (user is null)
        {
            throw new NotFoundException("User not found.");
        }

        user.IsSuspended = false;
        user.SuspensionReason = null;
        user.UpdatedAt = _clock.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        await _audit.RecordAsync("UserReactivated", nameof(User), user.Id.ToString(), null, null, ct);
        return Result.Ok();
    }

    public async Task<Result> AssignRoleAsync(int userId, AssignRoleRequest request, CancellationToken ct = default)
    {
        RequireAdmin();

        if (!Enum.TryParse<RoleName>(request.Role, true, out var role) || role == 0)
        {
            return Result.Fail("Unknown role.");
        }

        var roleEntity = await _unitOfWork.Repository<Role>().Query().FirstOrDefaultAsync(r => r.Name == role, ct);
        if (roleEntity is null)
        {
            return Result.Fail("Role is not registered on the platform.");
        }

        var user = await _unitOfWork.Repository<User>().Query().FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, ct);
        if (user is null)
        {
            throw new NotFoundException("User not found.");
        }

        var exists = await _unitOfWork.Repository<UserRole>().Query().AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleEntity.Id, ct);
        if (exists)
        {
            return Result.Fail("The user already holds this role.");
        }

        await _unitOfWork.Repository<UserRole>().AddAsync(new UserRole
        {
            UserId = userId,
            RoleId = roleEntity.Id,
            GrantedByUserId = _current.UserId,
            GrantedAt = _clock.UtcNow
        }, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        await _audit.RecordAsync("RoleAssigned", nameof(UserRole), $"user:{userId}",
            request.Justification, new { role = role.ToString() }, ct);
        return Result.Ok();
    }

    public async Task<Result> RevokeRoleAsync(int userId, string role, CancellationToken ct = default)
    {
        RequireAdmin();

        if (!Enum.TryParse<RoleName>(role, true, out var roleName) || roleName == 0)
        {
            return Result.Fail("Unknown role.");
        }

        if (roleName == RoleName.Member)
        {
            return Result.Fail("The base Member role cannot be revoked.");
        }

        if (roleName == RoleName.Administrator && userId == _current.UserId)
        {
            return Result.Fail("You cannot revoke your own Administrator role.");
        }

        var userRole = await _unitOfWork.Repository<UserRole>().Query()
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.UserId == userId && x.Role.Name == roleName, ct);
        if (userRole is null)
        {
            return Result.Fail("The user does not hold this role.");
        }

        if (roleName == RoleName.Administrator)
        {
            var otherAdmins = await _unitOfWork.Repository<UserRole>().Query().AnyAsync(
                x => x.UserId != userId && x.Role.Name == RoleName.Administrator &&
                     !x.User.IsDeleted && !x.User.IsSuspended, ct);
            if (!otherAdmins)
            {
                return Result.Fail("Cannot revoke the last active Administrator role.");
            }
        }

        _unitOfWork.Repository<UserRole>().Remove(userRole);
        await _unitOfWork.SaveChangesAsync(ct);

        await _audit.RecordAsync("RoleRevoked", nameof(UserRole), $"user:{userId}",
            null, new { role = roleName.ToString() }, ct);
        return Result.Ok();
    }

    public async Task<PagedResult<AuditLogDto>> GetAuditLogsAsync(int page, int pageSize, CancellationToken ct = default)
    {
        RequireAdmin();

        var (p, s) = Pagination.Normalize(page, pageSize);
        var q = _unitOfWork.Repository<AuditLog>().Query().OrderByDescending(a => a.CreatedAt);
        var total = await q.CountAsync(ct);

        var items = await q
            .Skip((p - 1) * s)
            .Take(s)
            .Select(a => new AuditLogDto
            {
                Id = a.Id,
                ActorUserId = a.ActorUserId,
                Action = a.Action,
                EntityName = a.EntityName,
                EntityId = a.EntityId,
                Justification = a.Justification,
                Metadata = a.Metadata,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync(ct);

        return PagedResult<AuditLogDto>.Create(items, p, s, total);
    }

    public async Task<IReadOnlyList<PlatformSettingDto>> GetSettingsAsync(CancellationToken ct = default)
    {
        RequireAdmin();

        return await _unitOfWork.Repository<PlatformSetting>().Query()
            .OrderBy(p => p.Key)
            .Select(p => new PlatformSettingDto(p.Key, p.Value, p.Description))
            .ToListAsync(ct);
    }

    public async Task<Result> UpdateSettingAsync(PlatformSettingUpdateRequest request, CancellationToken ct = default)
    {
        RequireAdmin();

        var setting = await _unitOfWork.Repository<PlatformSetting>().Query().FirstOrDefaultAsync(p => p.Key == request.Key, ct);
        if (setting is null)
        {
            await _unitOfWork.Repository<PlatformSetting>().AddAsync(new PlatformSetting
            {
                Key = request.Key,
                Value = request.Value,
                Description = request.Description
            }, ct);
        }
        else
        {
            setting.Value = request.Value;
            setting.Description = request.Description;
        }

        await _unitOfWork.SaveChangesAsync(ct);

        await _audit.RecordAsync("PlatformSettingUpdated", nameof(PlatformSetting),
            request.Key, null, null, ct);
        return Result.Ok();
    }

    public async Task<AdminDashboardDto> GetDashboardAsync(CancellationToken ct = default)
    {
        RequireAdmin();

        // EF Core is not thread-safe: run counts sequentially on the shared scoped context.
        var totalIdeas = await _unitOfWork.Repository<Idea>().Query().CountAsync(i => !i.IsDeleted, ct);
        var published = await _unitOfWork.Repository<Idea>().Query().CountAsync(i => !i.IsDeleted && i.Status == IdeaStatus.Published, ct);
        var pending = await _unitOfWork.Repository<Idea>().Query().CountAsync(i => !i.IsDeleted && PendingStatuses.Contains(i.Status), ct);
        var rejected = await _unitOfWork.Repository<Idea>().Query().CountAsync(i => !i.IsDeleted && i.Status == IdeaStatus.Rejected, ct);
        var shortlisted = await _unitOfWork.Repository<Idea>().Query().CountAsync(i => !i.IsDeleted && i.Status == IdeaStatus.Shortlisted, ct);
        var users = await _unitOfWork.Repository<User>().Query().CountAsync(u => !u.IsDeleted, ct);
        var entrepreneurs = await _unitOfWork.Repository<EntrepreneurProfile>().Query().CountAsync(ct);
        var anonymousReviews = await _unitOfWork.Repository<IdeaReview>().Query().CountAsync(r => !r.IsDeleted && r.ReviewerUserId == null, ct);
        var registeredReviews = await _unitOfWork.Repository<IdeaReview>().Query().CountAsync(r => !r.IsDeleted && r.ReviewerUserId != null, ct);
        var votes = await _unitOfWork.Repository<IdeaVote>().Query().CountAsync(v => !v.IsDeleted, ct);
        var inquiries = await _unitOfWork.Repository<EntrepreneurInquiry>().Query().CountAsync(ct);
        var openReports = await _unitOfWork.Repository<Report>().Query().CountAsync(r => r.Status == ReportStatus.Open, ct);
        var listings = await _unitOfWork.Repository<MarketplaceListing>().Query().CountAsync(l => !l.IsDeleted, ct);

        const string ideasPerMonthSql = """
            SELECT to_char(date_trunc('month', created_at), 'YYYY-MM') AS "Label",
                   count(*)::text AS "Value"
            FROM ideas
            WHERE is_deleted = false
            GROUP BY date_trunc('month', created_at)
            ORDER BY date_trunc('month', created_at) DESC
            LIMIT 12
            """;
        // The Dapper query owns its own connection, so it can safely run in parallel.
        var ideasPerMonth = await _unitOfWork.Repository<Idea>().ExecuteQueryAsync<DashboardMetricDto>(ideasPerMonthSql, null, ct);

        return new AdminDashboardDto
        {
            TotalIdeas = totalIdeas,
            PublishedIdeas = published,
            PendingIdeas = pending,
            RejectedIdeas = rejected,
            ShortlistedIdeas = shortlisted,
            TotalUsers = users,
            TotalEntrepreneurs = entrepreneurs,
            AnonymousReviewCount = anonymousReviews,
            RegisteredReviewCount = registeredReviews,
            TotalVotes = votes,
            TotalInquiries = inquiries,
            OpenReports = openReports,
            MarketplaceListings = listings,
            IdeasPerMonth = ideasPerMonth
        };
    }

    private void RequireAdmin()
    {
        if (!_current.IsAdmin)
        {
            throw new ForbiddenException();
        }
    }
}


