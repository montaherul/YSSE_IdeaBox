using Microsoft.EntityFrameworkCore;
using YSSE.IdeaBox.Application.Abstractions;
using YSSE.IdeaBox.Application.Abstractions.Services;
using YSSE.IdeaBox.Application.Common;
using YSSE.IdeaBox.Domain.Entities;

namespace YSSE.IdeaBox.Application.Services.Identity;

public sealed class RolePermissionService : IRolePermissionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _clock;
    private readonly IAuditWriter _audit;

    public RolePermissionService(IUnitOfWork unitOfWork, IDateTimeProvider clock, IAuditWriter audit)
    {
        _unitOfWork = unitOfWork;
        _clock = clock;
        _audit = audit;
    }

    public async Task<Result> AssignPermissionToRoleAsync(int roleId, int permissionId, CancellationToken ct = default)
    {
        var role = await _unitOfWork.Repository<Role>().Query().FirstOrDefaultAsync(r => r.Id == roleId, ct);
        if (role is null)
        {
            throw new NotFoundException("Role not found.");
        }

        var permission = await _unitOfWork.Repository<Permission>().Query().FirstOrDefaultAsync(p => p.Id == permissionId, ct);
        if (permission is null)
        {
            throw new NotFoundException("Permission not found.");
        }

        var exists = await _unitOfWork.Repository<RolePermission>().Query().AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId, ct);
        if (exists)
        {
            return Result.Fail("Permission already assigned to this role.");
        }

        await _unitOfWork.Repository<RolePermission>().AddAsync(new RolePermission
        {
            RoleId = roleId,
            PermissionId = permissionId,
            CreatedAt = _clock.UtcNow
        }, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        await _audit.RecordAsync("PermissionAssignedToRole", nameof(RolePermission), $"{roleId}:{permissionId}", null, new { roleId, permissionId }, ct);
        return Result.Ok();
    }

    public async Task<Result> RemovePermissionFromRoleAsync(int roleId, int permissionId, CancellationToken ct = default)
    {
        var rolePermission = await _unitOfWork.Repository<RolePermission>().Query()
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId, ct);
        if (rolePermission is null)
        {
            return Result.Fail("Permission not assigned to this role.");
        }

        _unitOfWork.Repository<RolePermission>().Remove(rolePermission);
        await _unitOfWork.SaveChangesAsync(ct);

        await _audit.RecordAsync("PermissionRemovedFromRole", nameof(RolePermission), $"{roleId}:{permissionId}", null, new { roleId, permissionId }, ct);
        return Result.Ok();
    }
}
