using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YSSE.IdeaBox.Application.Abstractions.Services;
using YSSE.IdeaBox.Application.Common;
using YSSE.IdeaBox.Application.Dtos;
using YSSE.IdeaBox.Domain.Enums;

namespace YSSE.IdeaBox.Api.Controllers;

[Route("api/v1/admin")]
[Authorize(Policy = "Admin")]
public sealed class AdminController : ApiControllerBase
{
    private readonly IAdminService _admin;
    private readonly IReportService _reports;

    public AdminController(IAdminService admin, IReportService reports)
    {
        _admin = admin;
        _reports = reports;
    }

    [HttpGet("users")]
    public async Task<IActionResult> Users([FromQuery] AdminUserQuery query, CancellationToken ct)
        => Paged(await _admin.GetUsersAsync(query, ct));

    [HttpPost("users/{id:int}/suspend")]
    public async Task<IActionResult> Suspend(int id, [FromBody] SuspendUserRequest request, CancellationToken ct)
        => OkResult(await _admin.SuspendUserAsync(id, request, ct));

    [HttpPost("users/{id:int}/reactivate")]
    public async Task<IActionResult> Reactivate(int id, CancellationToken ct)
        => OkResult(await _admin.ReactivateUserAsync(id, ct));

    [HttpPost("users/{id:int}/roles")]
    public async Task<IActionResult> AssignRole(int id, [FromBody] AssignRoleRequest request, CancellationToken ct)
        => OkResult(await _admin.AssignRoleAsync(id, request, ct));

    [HttpDelete("users/{id:int}/roles/{role}")]
    public async Task<IActionResult> RevokeRole(int id, string role, CancellationToken ct)
        => OkResult(await _admin.RevokeRoleAsync(id, role, ct));

    [HttpGet("audit-logs")]
    public async Task<IActionResult> AuditLogs([FromQuery] int page = 1,
        [FromQuery] int pageSize = Pagination.DefaultPageSize, CancellationToken ct = default)
        => Paged(await _admin.GetAuditLogsAsync(page, pageSize, ct));

    [HttpGet("settings")]
    public async Task<IActionResult> GetSettings(CancellationToken ct)
    {
        var settings = await _admin.GetSettingsAsync(ct);
        var map = settings.ToDictionary(s => s.Key, s => s.Value);
        return OkData(map);
    }

    [HttpPut("settings")]
    public async Task<IActionResult> UpdateSettings([FromBody] Dictionary<string, JsonElement> values, CancellationToken ct)
    {
        foreach (var (key, element) in values)
        {
            var value = element.ValueKind == JsonValueKind.Null ? string.Empty : element.ToString();
            var result = await _admin.UpdateSettingAsync(new PlatformSettingUpdateRequest { Key = key, Value = value }, ct);
            if (!result.Success)
            {
                return Failed(result.Message);
            }
        }

        return OkResult(Result.Ok("Settings updated."));
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard(CancellationToken ct)
        => OkData(await _admin.GetDashboardAsync(ct));

    [HttpGet("reports")]
    [Authorize(Policy = "Moderator")]
    public async Task<IActionResult> Reports([FromQuery] ReportStatus? status, [FromQuery] int page = 1,
        [FromQuery] int pageSize = Pagination.DefaultPageSize, CancellationToken ct = default)
        => Paged(await _reports.ListAsync(status, page, pageSize, ct));

    [HttpPost("reports/{id:int}/resolve")]
    [Authorize(Policy = "Moderator")]
    public async Task<IActionResult> ResolveReport(int id, [FromBody] ReportResolveRequest request, CancellationToken ct)
        => OkResult(await _reports.ResolveAsync(id, request, ct));
}
