using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YSSE.IdeaBox.Application.Abstractions.Services;
using YSSE.IdeaBox.Application.Common;
using YSSE.IdeaBox.Application.Dtos;

namespace YSSE.IdeaBox.Api.Controllers;

[Route("api/v1/roles")]
[Authorize]
public sealed class RolesController : ApiControllerBase
{
    private readonly IRoleService _roles;
    private readonly IRolePermissionService _rolePermissions;

    public RolesController(IRoleService roles, IRolePermissionService rolePermissions)
    {
        _roles = roles;
        _rolePermissions = rolePermissions;
    }

    [HttpGet]
    [Authorize(Policy = Permissions.RolesRead)]
    public async Task<IActionResult> GetRoles(CancellationToken ct)
        => OkData(await _roles.GetRolesAsync(ct));

    [HttpGet("{id:int}")]
    [Authorize(Policy = Permissions.RolesRead)]
    public async Task<IActionResult> GetRole(int id, CancellationToken ct)
    {
        var role = await _roles.GetRoleByIdAsync(id, ct);
        return role is null ? NotFoundError("Role not found.") : OkData(role);
    }

    [HttpGet("{id:int}/users")]
    [Authorize(Policy = Permissions.RolesRead)]
    public async Task<IActionResult> GetRoleUsers(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null, CancellationToken ct = default)
        => Paged(await _roles.GetRoleUsersAsync(id, page, pageSize, search, ct));

    [HttpPost]
    [Authorize(Policy = Permissions.RolesManage)]
    public async Task<IActionResult> CreateRole([FromBody] RoleUpsertRequest request, CancellationToken ct)
        => OkResult(await _roles.CreateRoleAsync(request, ct));

    [HttpPut("{id:int}")]
    [Authorize(Policy = Permissions.RolesManage)]
    public async Task<IActionResult> UpdateRole(int id, [FromBody] RoleUpsertRequest request, CancellationToken ct)
        => OkResult(await _roles.UpdateRoleAsync(id, request, ct));

    [HttpDelete("{id:int}")]
    [Authorize(Policy = Permissions.RolesManage)]
    public async Task<IActionResult> DeleteRole(int id, CancellationToken ct)
        => OkResult(await _roles.DeleteRoleAsync(id, ct));

    [HttpPost("{roleId:int}/permissions/{permissionId:int}")]
    [Authorize(Policy = Permissions.RolesManage)]
    public async Task<IActionResult> AssignPermission(int roleId, int permissionId, CancellationToken ct)
        => OkResult(await _rolePermissions.AssignPermissionToRoleAsync(roleId, permissionId, ct));

    [HttpDelete("{roleId:int}/permissions/{permissionId:int}")]
    [Authorize(Policy = Permissions.RolesManage)]
    public async Task<IActionResult> RemovePermission(int roleId, int permissionId, CancellationToken ct)
        => OkResult(await _rolePermissions.RemovePermissionFromRoleAsync(roleId, permissionId, ct));
}