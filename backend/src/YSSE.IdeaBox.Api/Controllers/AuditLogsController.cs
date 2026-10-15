using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YSSE.IdeaBox.Application.Abstractions.Services;
using YSSE.IdeaBox.Application.Common;

namespace YSSE.IdeaBox.Api.Controllers;

[Route("api/v1/audit-logs")]
[Authorize]
public sealed class AuditLogsController : ApiControllerBase
{
    private readonly IAuditLogService _auditLogs;

    public AuditLogsController(IAuditLogService auditLogs) => _auditLogs = auditLogs;

    [HttpGet]
    [Authorize(Policy = Permissions.AuditRead)]
    public async Task<IActionResult> AuditLogs([FromQuery] int page = 1,
        [FromQuery] int pageSize = Pagination.DefaultPageSize, CancellationToken ct = default)
        => Paged(await _auditLogs.GetAuditLogsAsync(page, pageSize, ct));
}