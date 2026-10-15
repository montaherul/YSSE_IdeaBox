using Microsoft.EntityFrameworkCore;
using YSSE.IdeaBox.Application.Abstractions;
using YSSE.IdeaBox.Application.Abstractions.Services;
using YSSE.IdeaBox.Application.Common;
using YSSE.IdeaBox.Application.Dtos;
using YSSE.IdeaBox.Domain.Entities;
using YSSE.IdeaBox.Domain.Enums;

namespace YSSE.IdeaBox.Application.Services.Identity;

public sealed class UserManagementService : IUserManagementService
{
    private static readonly System.Linq.Expressions.Expression<Func<User, AdminUserDto>> ToAdminUser = u => new AdminUserDto
    {
        Id = u.Id,
        DisplayName = u.DisplayName,
        Email = u.Email,
        PhoneNumber = u.PhoneNumber,
        IsSuspended = u.IsSuspended,
        SuspensionReason = u.SuspensionReason,
        IsEmailVerified = u.IsEmailVerified,
        Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList(),
        CreatedAt = u.CreatedAt
    };

    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _current;
    private readonly IDateTimeProvider _clock;
    private readonly IAuditWriter _audit;
    private readonly IPasswordHasher _passwordHasher;

    public UserManagementService(
        IUnitOfWork unitOfWork,
        ICurrentUserService current,
        IDateTimeProvider clock,
        IAuditWriter audit,
        IPasswordHasher passwordHasher)
    {
        _unitOfWork = unitOfWork;
        _current = current;
        _clock = clock;
        _audit = audit;
        _passwordHasher = passwordHasher;
    }

    public async Task<PagedResult<AdminUserDto>> GetUsersAsync(AdminUserQuery query, CancellationToken ct = default)
    {
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
            var roleName = query.Role.Trim().ToLower();
            q = q.Where(u => u.UserRoles.Any(ur => ur.Role.Name.ToLower() == roleName));
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
            .Select(ToAdminUser)
            .ToListAsync(ct);

        return PagedResult<AdminUserDto>.Create(items, page, pageSize, total);
    }

    public async Task<AdminUserDto?> GetUserByIdAsync(int userId, CancellationToken ct = default)
    {
        return await _unitOfWork.Repository<User>().Query()
            .Where(u => u.Id == userId && !u.IsDeleted)
            .Select(ToAdminUser)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<Result<int>> CreateUserAsync(UserCreateRequest request, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var phone = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();

        var emailTaken = await _unitOfWork.Repository<User>().Query()
            .AnyAsync(u => !u.IsDeleted && u.Email != null && u.Email.ToLower() == email, ct);
        if (emailTaken)
        {
            throw new ConflictException("A user with this email already exists.");
        }

        if (phone is not null)
        {
            var phoneTaken = await _unitOfWork.Repository<User>().Query()
                .AnyAsync(u => !u.IsDeleted && u.PhoneNumber == phone, ct);
            if (phoneTaken)
            {
                throw new ConflictException("A user with this phone number already exists.");
            }
        }

        var user = new User
        {
            Email = email,
            PhoneNumber = phone,
            PasswordHash = _passwordHasher.Hash(request.Password),
            DisplayName = request.DisplayName.Trim(),
            IsEmailVerified = true,
            CreatedAt = _clock.UtcNow
        };
        await _unitOfWork.Repository<User>().AddAsync(user, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        await _unitOfWork.Repository<MemberProfile>().AddAsync(new MemberProfile
        {
            UserId = user.Id,
            DisplayName = user.DisplayName,
            PreferredLanguage = "en",
            CreatedAt = _clock.UtcNow
        }, ct);

        var requestedNames = request.Roles
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .Select(r => r.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        requestedNames.Add(SystemRoles.Member);

        var roles = (await _unitOfWork.Repository<Role>().Query().ToListAsync(ct))
            .Where(r => requestedNames.Contains(r.Name))
            .ToList();
        foreach (var role in roles)
        {
            await _unitOfWork.Repository<UserRole>().AddAsync(new UserRole
            {
                UserId = user.Id,
                RoleId = role.Id,
                GrantedByUserId = _current.UserId,
                GrantedAt = _clock.UtcNow
            }, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);

        await _audit.RecordAsync("UserCreated", nameof(User), user.Id.ToString(), null,
            new { user.Email, Roles = roles.Select(r => r.Name).ToArray() }, ct);
        return Result<int>.Ok(user.Id);
    }

    public async Task<Result> UpdateUserAsync(int userId, UserUpdateRequest request, CancellationToken ct = default)
    {
        var user = await _unitOfWork.Repository<User>().Query()
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, ct);
        if (user is null)
        {
            throw new NotFoundException("User not found.");
        }

        var email = string.IsNullOrWhiteSpace(request.Email) ? user.Email : request.Email.Trim().ToLowerInvariant();
        if (email is not null && !string.Equals(email, user.Email, StringComparison.OrdinalIgnoreCase))
        {
            var emailTaken = await _unitOfWork.Repository<User>().Query()
                .AnyAsync(u => u.Id != userId && !u.IsDeleted && u.Email != null && u.Email.ToLower() == email, ct);
            if (emailTaken)
            {
                throw new ConflictException("A user with this email already exists.");
            }
        }

        var phone = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();
        if (phone is not null && !string.Equals(phone, user.PhoneNumber, StringComparison.Ordinal))
        {
            var phoneTaken = await _unitOfWork.Repository<User>().Query()
                .AnyAsync(u => u.Id != userId && !u.IsDeleted && u.PhoneNumber == phone, ct);
            if (phoneTaken)
            {
                throw new ConflictException("A user with this phone number already exists.");
            }
        }

        user.DisplayName = request.DisplayName.Trim();
        user.Email = email;
        user.PhoneNumber = phone;
        if (request.IsEmailVerified is not null)
        {
            user.IsEmailVerified = request.IsEmailVerified.Value;
        }

        user.UpdatedAt = _clock.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        await _audit.RecordAsync("UserUpdated", nameof(User), user.Id.ToString(), null,
            new { user.Email, user.DisplayName }, ct);
        return Result.Ok();
    }

    public async Task<Result> DeleteUserAsync(int userId, CancellationToken ct = default)
    {
        if (userId == _current.UserId)
        {
            return Result.Fail("You cannot delete your own account.");
        }

        var user = await _unitOfWork.Repository<User>().Query()
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, ct);
        if (user is null)
        {
            throw new NotFoundException("User not found.");
        }

        var holdsAdmin = await _unitOfWork.Repository<UserRole>().Query()
            .AnyAsync(ur => ur.UserId == userId && ur.Role.Name == SystemRoles.Administrator, ct);
        if (holdsAdmin)
        {
            var anotherActiveAdmin = await _unitOfWork.Repository<UserRole>().Query()
                .AnyAsync(ur => ur.UserId != userId && ur.Role.Name == SystemRoles.Administrator &&
                                !ur.User.IsDeleted && !ur.User.IsSuspended, ct);
            if (!anotherActiveAdmin)
            {
                return Result.Fail("Cannot delete the last active Administrator.");
            }
        }

        var tokens = await _unitOfWork.Repository<RefreshToken>().Query()
            .Where(t => t.UserId == userId).ToListAsync(ct);
        if (tokens.Count > 0)
        {
            _unitOfWork.Repository<RefreshToken>().RemoveRange(tokens);
        }

        var userRoles = await _unitOfWork.Repository<UserRole>().Query()
            .Where(ur => ur.UserId == userId).ToListAsync(ct);
        if (userRoles.Count > 0)
        {
            _unitOfWork.Repository<UserRole>().RemoveRange(userRoles);
        }

        var memberProfile = await _unitOfWork.Repository<MemberProfile>().Query()
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);
        if (memberProfile is not null)
        {
            _unitOfWork.Repository<MemberProfile>().Remove(memberProfile);
        }

        var creatorProfile = await _unitOfWork.Repository<CreatorProfile>().Query()
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);
        if (creatorProfile is not null)
        {
            _unitOfWork.Repository<CreatorProfile>().Remove(creatorProfile);
        }

        var entrepreneurProfile = await _unitOfWork.Repository<EntrepreneurProfile>().Query()
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);
        if (entrepreneurProfile is not null)
        {
            _unitOfWork.Repository<EntrepreneurProfile>().Remove(entrepreneurProfile);
        }

        _unitOfWork.Repository<User>().Remove(user);

        try
        {
            await _unitOfWork.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            throw new ConflictException("This user has related records and cannot be deleted. Suspend the account instead.");
        }

        await _audit.RecordAsync("UserDeleted", nameof(User), userId.ToString(), null, new { user.Email }, ct);
        return Result.Ok();
    }

    public async Task<Result> SuspendUserAsync(int userId, SuspendUserRequest request, CancellationToken ct = default)
    {
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
}
