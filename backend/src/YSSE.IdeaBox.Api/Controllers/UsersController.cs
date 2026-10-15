using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YSSE.IdeaBox.Application.Abstractions.Services;
using YSSE.IdeaBox.Application.Common;
using YSSE.IdeaBox.Application.Dtos;

namespace YSSE.IdeaBox.Api.Controllers;

/// <summary>
/// User management resource endpoints.
/// Uses permission-based authorization instead of role-based.
/// </summary>
[Route("api/v1/users")]
[Authorize]
public sealed class UsersController : ApiControllerBase
{
    private readonly IUserManagementService _users;
    private readonly IUserRoleService _roles;

    public UsersController(IUserManagementService users, IUserRoleService roles)
    {
        _users = users;
        _roles = roles;
    }

    [HttpGet]
    [Authorize(Policy = Permissions.UsersRead)]
    public async Task<IActionResult> List([FromQuery] AdminUserQuery query, CancellationToken ct)
        => Paged(await _users.GetUsersAsync(query, ct));

    [HttpGet("{id:int}")]
    [Authorize(Policy = Permissions.UsersRead)]
    public async Task<IActionResult> Get(int id, CancellationToken ct)
    {
        var user = await _users.GetUserByIdAsync(id, ct);
        return user is null ? NotFoundError("User not found.") : OkData(user);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.UsersCreate)]
    public async Task<IActionResult> Create([FromBody] UserCreateRequest request, CancellationToken ct)
        => OkResult(await _users.CreateUserAsync(request, ct));

    [HttpPut("{id:int}")]
    [Authorize(Policy = Permissions.UsersUpdate)]
    public async Task<IActionResult> Update(int id, [FromBody] UserUpdateRequest request, CancellationToken ct)
        => OkResult(await _users.UpdateUserAsync(id, request, ct));

    [HttpDelete("{id:int}")]
    [Authorize(Policy = Permissions.UsersDelete)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
        => OkResult(await _users.DeleteUserAsync(id, ct));

    [HttpPost("{id:int}/suspend")]
    [Authorize(Policy = Permissions.UsersSuspend)]
    public async Task<IActionResult> Suspend(int id, [FromBody] SuspendUserRequest request, CancellationToken ct)
        => OkResult(await _users.SuspendUserAsync(id, request, ct));

    [HttpPost("{id:int}/reactivate")]
    [Authorize(Policy = Permissions.UsersSuspend)]
    public async Task<IActionResult> Reactivate(int id, CancellationToken ct)
        => OkResult(await _users.ReactivateUserAsync(id, ct));

    [HttpPost("{id:int}/roles")]
    [Authorize(Policy = Permissions.UsersManageRoles)]
    public async Task<IActionResult> AssignRole(int id, [FromBody] AssignRoleRequest request, CancellationToken ct)
        => OkResult(await _roles.AssignRoleAsync(id, request, ct));

    [HttpDelete("{id:int}/roles/{role}")]
    [Authorize(Policy = Permissions.UsersManageRoles)]
    public async Task<IActionResult> RevokeRole(int id, string role, CancellationToken ct)
        => OkResult(await _roles.RevokeRoleAsync(id, role, ct));
}