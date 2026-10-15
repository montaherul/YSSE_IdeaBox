using YSSE.IdeaBox.Application.Common;
using YSSE.IdeaBox.Application.Dtos;

namespace YSSE.IdeaBox.Application.Abstractions.Services;

/// <summary>Role catalog management. System roles (Administrator, Member) are protected.</summary>
public interface IRoleService
{
    Task<IReadOnlyList<RoleDto>> GetRolesAsync(CancellationToken ct = default);

    Task<RoleDto?> GetRoleByIdAsync(int id, CancellationToken ct = default);

    Task<PagedResult<AdminUserDto>> GetRoleUsersAsync(int roleId, int page, int pageSize, string? search, CancellationToken ct = default);

    Task<Result<int>> CreateRoleAsync(RoleUpsertRequest request, CancellationToken ct = default);

    Task<Result> UpdateRoleAsync(int id, RoleUpsertRequest request, CancellationToken ct = default);

    Task<Result> DeleteRoleAsync(int id, CancellationToken ct = default);
}

/// <summary>Permission catalog management.</summary>
public interface IPermissionService
{
    Task<IReadOnlyList<PermissionDto>> GetPermissionsAsync(CancellationToken ct = default);

    Task<PagedResult<PermissionDto>> GetPermissionsPagedAsync(int page, int pageSize, string? search, CancellationToken ct = default);

    Task<PermissionDto?> GetPermissionByIdAsync(int id, CancellationToken ct = default);

    Task<Result<int>> CreatePermissionAsync(PermissionUpsertRequest request, CancellationToken ct = default);

    Task<Result> UpdatePermissionAsync(int id, PermissionUpsertRequest request, CancellationToken ct = default);

    Task<Result> DeletePermissionAsync(int id, CancellationToken ct = default);
}

/// <summary>Assignment of permissions to roles (resource: roles/{roleId}/permissions).</summary>
public interface IRolePermissionService
{
    Task<Result> AssignPermissionToRoleAsync(int roleId, int permissionId, CancellationToken ct = default);

    Task<Result> RemovePermissionFromRoleAsync(int roleId, int permissionId, CancellationToken ct = default);
}
