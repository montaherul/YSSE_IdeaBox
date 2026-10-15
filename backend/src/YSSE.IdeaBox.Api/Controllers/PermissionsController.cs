using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YSSE.IdeaBox.Application.Abstractions.Services;
using YSSE.IdeaBox.Application.Common;
using YSSE.IdeaBox.Application.Dtos;

namespace YSSE.IdeaBox.Api.Controllers;

[Route("api/v1/permissions")]
[Authorize]
public sealed class PermissionsController : ApiControllerBase
{
    private readonly IPermissionService _permissions;

    public PermissionsController(IPermissionService permissions) => _permissions = permissions;

    [HttpGet]
    [Authorize(Policy = Permissions.PermissionsRead)]
    public async Task<IActionResult> GetPermissions(CancellationToken ct)
        => OkData(await _permissions.GetPermissionsAsync(ct));

    [HttpGet("paged")]
    [Authorize(Policy = Permissions.PermissionsRead)]
    public async Task<IActionResult> GetPermissionsPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = Pagination.DefaultPageSize,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
        => Paged(await _permissions.GetPermissionsPagedAsync(page, pageSize, search, ct));

    [HttpGet("{id:int}")]
    [Authorize(Policy = Permissions.PermissionsRead)]
    public async Task<IActionResult> GetPermission(int id, CancellationToken ct)
    {
        var permission = await _permissions.GetPermissionByIdAsync(id, ct);
        return permission is null ? NotFoundError("Permission not found.") : OkData(permission);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.PermissionsManage)]
    public async Task<IActionResult> CreatePermission([FromBody] PermissionUpsertRequest request, CancellationToken ct)
        => OkResult(await _permissions.CreatePermissionAsync(request, ct));

    [HttpPut("{id:int}")]
    [Authorize(Policy = Permissions.PermissionsManage)]
    public async Task<IActionResult> UpdatePermission(int id, [FromBody] PermissionUpsertRequest request, CancellationToken ct)
        => OkResult(await _permissions.UpdatePermissionAsync(id, request, ct));

    [HttpDelete("{id:int}")]
    [Authorize(Policy = Permissions.PermissionsManage)]
    public async Task<IActionResult> DeletePermission(int id, CancellationToken ct)
        => OkResult(await _permissions.DeletePermissionAsync(id, ct));
}