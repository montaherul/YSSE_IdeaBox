using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YSSE.IdeaBox.Application.Abstractions.Services;
using YSSE.IdeaBox.Application.Common;

namespace YSSE.IdeaBox.Api.Controllers;

[Route("api/v1/dashboard")]
[Authorize]
public sealed class DashboardController : ApiControllerBase
{
    private readonly IAdminDashboardService _dashboard;

    public DashboardController(IAdminDashboardService dashboard) => _dashboard = dashboard;

    [HttpGet]
    [Authorize(Policy = Permissions.DashboardRead)]
    public async Task<IActionResult> Dashboard(CancellationToken ct)
        => OkData(await _dashboard.GetDashboardAsync(ct));
}