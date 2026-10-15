using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YSSE.IdeaBox.Application.Abstractions.Services;
using YSSE.IdeaBox.Application.Common;
using YSSE.IdeaBox.Application.Dtos;
using YSSE.IdeaBox.Domain.Enums;

namespace YSSE.IdeaBox.Api.Controllers;

/// <summary>
/// Reports resource. Submission is open to any authenticated member; the moderation
/// queue and resolution are gated by content-report policies. Access is enforced by
/// policy per action, not by the route.
/// </summary>
[Route("api/v1/reports")]
public sealed class ReportsController : ApiControllerBase
{
    private readonly IReportService _reports;

    public ReportsController(IReportService reports) => _reports = reports;

    [HttpGet]
    [Authorize(Policy = Permissions.ContentReportsRead)]
    public async Task<IActionResult> List([FromQuery] ReportStatus? status, [FromQuery] int page = 1,
        [FromQuery] int pageSize = Pagination.DefaultPageSize, CancellationToken ct = default)
        => Paged(await _reports.ListAsync(status, page, pageSize, ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ReportCreateRequest request, CancellationToken ct)
        => OkResult(await _reports.CreateAsync(request, ct));

    [HttpPost("{id:int}/resolve")]
    [Authorize(Policy = Permissions.ContentReportsManage)]
    public async Task<IActionResult> Resolve(int id, [FromBody] ReportResolveRequest request, CancellationToken ct)
        => OkResult(await _reports.ResolveAsync(id, request, ct));

    [HttpPut("{id:int}")]
    [Authorize(Policy = Permissions.ContentReportsManage)]
    public async Task<IActionResult> Update(int id, [FromBody] ReportUpdateRequest request, CancellationToken ct)
        => OkResult(await _reports.UpdateAsync(id, request, ct));

    [HttpDelete("{id:int}")]
    [Authorize(Policy = Permissions.ContentReportsManage)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
        => OkResult(await _reports.DeleteAsync(id, ct));
}