using Microsoft.EntityFrameworkCore;
using YSSE.IdeaBox.Application.Abstractions;
using YSSE.IdeaBox.Application.Abstractions.Services;
using YSSE.IdeaBox.Application.Common;
using YSSE.IdeaBox.Application.Dtos;
using YSSE.IdeaBox.Domain.Entities;
using YSSE.IdeaBox.Domain.Enums;

namespace YSSE.IdeaBox.Application.Services.Identity;

public sealed class RoleService : IRoleService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _clock;
    private readonly IAuditWriter _audit;

    public RoleService(IUnitOfWork unitOfWork, IDateTimeProvider clock, IAuditWriter audit)
    {
        _unitOfWork = unitOfWork;
        _clock = clock;
        _audit = audit;
    }

    public async Task<IReadOnlyList<RoleDto>> GetRolesAsync(CancellationToken ct = default)
    {
        return await _unitOfWork.Repository<Role>().Query()
            .OrderBy(r => r.Name)
            .Select(r => new RoleDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                Permissions = r.RolePermissions.Select(rp => new PermissionDto
                {
                    Id = rp.Permission.Id,
                    Code = rp.Permission.Code,
                    Description = rp.Permission.Description
                }).ToList(),
                UserCount = r.UserRoles.Count(ur => !ur.User.IsDeleted),
                CreatedAt = r.CreatedAt
            })
            .ToListAsync(ct);
    }

    public async Task<RoleDto?> GetRoleByIdAsync(int id, CancellationToken ct = default)
    {
        return await _unitOfWork.Repository<Role>().Query()
            .Where(r => r.Id == id)
            .Select(r => new RoleDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                Permissions = r.RolePermissions.Select(rp => new PermissionDto
                {
                    Id = rp.Permission.Id,
                    Code = rp.Permission.Code,
                    Description = rp.Permission.Description
                }).ToList(),
                UserCount = r.UserRoles.Count(ur => !ur.User.IsDeleted),
                CreatedAt = r.CreatedAt
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<PagedResult<AdminUserDto>> GetRoleUsersAsync(int roleId, int page, int pageSize, string? search, CancellationToken ct = default)
    {
        var roleExists = await _unitOfWork.Repository<Role>().Query().AnyAsync(r => r.Id == roleId, ct);
        if (!roleExists)
        {
            throw new NotFoundException("Role not found.");
        }

        var (p, s) = Pagination.Normalize(page, pageSize);
        var q = _unitOfWork.Repository<User>().Query()
            .Where(u => !u.IsDeleted && u.UserRoles.Any(ur => ur.RoleId == roleId));

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim().ToLower()}%";
            q = q.Where(u => EF.Functions.Like(u.DisplayName.ToLower(), pattern) ||
                             (u.Email != null && EF.Functions.Like(u.Email.ToLower(), pattern)));
        }

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(u => u.CreatedAt)
            .Skip((p - 1) * s)
            .Take(s)
            .Select(u => new AdminUserDto
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
            })
            .ToListAsync(ct);

        return PagedResult<AdminUserDto>.Create(items, p, s, total);
    }

    public async Task<Result<int>> CreateRoleAsync(RoleUpsertRequest request, CancellationToken ct = default)
    {
        var name = request.Name?.Trim() ?? string.Empty;
        if (name.Length == 0)
        {
            return Result<int>.Fail("Role name is required.");
        }

        if (name.Length > 100)
        {
            return Result<int>.Fail("Role name must be 100 characters or fewer.");
        }

        if (await _unitOfWork.Repository<Role>().Query().AnyAsync(r => r.Name.ToLower() == name.ToLower(), ct))
        {
            throw new ConflictException("A role with this name already exists.");
        }

        await using var transaction = await _unitOfWork.BeginTransactionAsync(ct);

        var role = new Role
        {
            Name = name,
            Description = request.Description?.Trim() ?? string.Empty,
            CreatedAt = _clock.UtcNow
        };
        await _unitOfWork.Repository<Role>().AddAsync(role, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        await SyncRolePermissionsAsync(role.Id, request.PermissionIds, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        await _unitOfWork.CommitAsync(transaction, ct);

        await _audit.RecordAsync("RoleCreated", nameof(Role), role.Id.ToString(), null,
            new { role.Name, PermissionIds = request.PermissionIds }, ct);
        return Result<int>.Ok(role.Id);
    }

    public async Task<Result> UpdateRoleAsync(int id, RoleUpsertRequest request, CancellationToken ct = default)
    {
        var role = await _unitOfWork.Repository<Role>().Query().FirstOrDefaultAsync(r => r.Id == id, ct);
        if (role is null)
        {
            throw new NotFoundException("Role not found.");
        }

        var name = request.Name?.Trim() ?? string.Empty;
        if (name.Length == 0)
        {
            return Result.Fail("Role name is required.");
        }

        if (name.Length > 100)
        {
            return Result.Fail("Role name must be 100 characters or fewer.");
        }

        var renaming = !string.Equals(role.Name, name, StringComparison.OrdinalIgnoreCase);
        if (renaming)
        {
            if (SystemRoles.IsSystemRole(role.Name))
            {
                return Result.Fail("System role names cannot be changed. Their description and permissions remain editable.");
            }

            if (await _unitOfWork.Repository<Role>().Query().AnyAsync(r => r.Id != id && r.Name.ToLower() == name.ToLower(), ct))
            {
                throw new ConflictException("Role name already in use.");
            }
        }

        await using var transaction = await _unitOfWork.BeginTransactionAsync(ct);

        role.Name = name;
        role.Description = request.Description?.Trim() ?? string.Empty;
        role.UpdatedAt = _clock.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        await SyncRolePermissionsAsync(role.Id, request.PermissionIds, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        await _unitOfWork.CommitAsync(transaction, ct);

        await _audit.RecordAsync("RoleUpdated", nameof(Role), role.Id.ToString(), null,
            new { role.Name, PermissionIds = request.PermissionIds }, ct);
        return Result.Ok();
    }

    public async Task<Result> DeleteRoleAsync(int id, CancellationToken ct = default)
    {
        var role = await _unitOfWork.Repository<Role>().Query().FirstOrDefaultAsync(r => r.Id == id, ct);
        if (role is null)
        {
            throw new NotFoundException("Role not found.");
        }

        if (SystemRoles.IsSystemRole(role.Name))
        {
            return Result.Fail("System roles cannot be deleted.");
        }

        var hasUsers = await _unitOfWork.Repository<UserRole>().Query().AnyAsync(ur => ur.RoleId == id && !ur.User.IsDeleted, ct);
        if (hasUsers)
        {
            return Result.Fail("Cannot delete role that is assigned to users.");
        }

        _unitOfWork.Repository<Role>().Remove(role);
        await _unitOfWork.SaveChangesAsync(ct);

        await _audit.RecordAsync("RoleDeleted", nameof(Role), role.Id.ToString(), null, new { role.Name }, ct);
        return Result.Ok();
    }

    /// <summary>
    /// Replaces a role's permission grants with the requested set. Unknown permission ids
    /// are ignored; existing grants not in the set are removed. Caller owns the transaction.
    /// </summary>
    private async Task SyncRolePermissionsAsync(int roleId, IReadOnlyList<int> requestedIds, CancellationToken ct)
    {
        var desired = requestedIds.Distinct().ToHashSet();

        var target = desired.Count == 0
            ? new HashSet<int>()
            : (await _unitOfWork.Repository<Permission>().Query()
                .Where(p => desired.Contains(p.Id))
                .Select(p => p.Id)
                .ToListAsync(ct)).ToHashSet();

        var current = await _unitOfWork.Repository<RolePermission>().Query()
            .Where(rp => rp.RoleId == roleId)
            .Select(rp => rp.PermissionId)
            .ToListAsync(ct);

        foreach (var permissionId in target.Except(current))
        {
            await _unitOfWork.Repository<RolePermission>().AddAsync(new RolePermission
            {
                RoleId = roleId,
                PermissionId = permissionId,
                CreatedAt = _clock.UtcNow
            }, ct);
        }

        var removeIds = current.Except(target).ToList();
        if (removeIds.Count > 0)
        {
            var toRemove = await _unitOfWork.Repository<RolePermission>().Query()
                .Where(rp => rp.RoleId == roleId && removeIds.Contains(rp.PermissionId))
                .ToListAsync(ct);
            _unitOfWork.Repository<RolePermission>().RemoveRange(toRemove);
        }
    }
}
