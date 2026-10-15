using YSSE.IdeaBox.Application.Common;
using YSSE.IdeaBox.Application.Dtos;

namespace YSSE.IdeaBox.Application.Abstractions.Services;

/// <summary>
/// Administrative management of platform accounts (resource: users).
/// Authorization is enforced at the API boundary by permission policies; this service
/// owns business invariants only (e.g. a caller cannot suspend their own account).
/// </summary>
public interface IUserManagementService
{
    Task<PagedResult<AdminUserDto>> GetUsersAsync(AdminUserQuery query, CancellationToken ct = default);

    Task<AdminUserDto?> GetUserByIdAsync(int userId, CancellationToken ct = default);

    Task<Result<int>> CreateUserAsync(UserCreateRequest request, CancellationToken ct = default);

    Task<Result> UpdateUserAsync(int userId, UserUpdateRequest request, CancellationToken ct = default);

    Task<Result> DeleteUserAsync(int userId, CancellationToken ct = default);

    Task<Result> SuspendUserAsync(int userId, SuspendUserRequest request, CancellationToken ct = default);

    Task<Result> ReactivateUserAsync(int userId, CancellationToken ct = default);
}

/// <summary>
/// Assignment and revocation of a user's roles (resource: users/{id}/roles).
/// Owns the protected-role invariants (base Member cannot be revoked, the last
/// active Administrator cannot be revoked).
/// </summary>
public interface IUserRoleService
{
    Task<Result> AssignRoleAsync(int userId, AssignRoleRequest request, CancellationToken ct = default);

    Task<Result> RevokeRoleAsync(int userId, string role, CancellationToken ct = default);
}
