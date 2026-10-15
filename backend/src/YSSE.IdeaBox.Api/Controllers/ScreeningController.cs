using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YSSE.IdeaBox.Application.Abstractions.Services;
using YSSE.IdeaBox.Application.Common;
using YSSE.IdeaBox.Application.Dtos;

namespace YSSE.IdeaBox.Api.Controllers;

[Route("api/v1/screening")]
[Authorize(Policy = Permissions.ScreeningRead)]
public sealed class ScreeningController : ApiControllerBase
{
    private readonly IScreeningService _screening;

    public ScreeningController(IScreeningService screening) => _screening = screening;

    [HttpGet("queue")]
    public async Task<IActionResult> Queue([FromQuery] IdeaListQuery query, CancellationToken ct)
        => Paged(await _screening.GetQueueAsync(query, ct));

    [HttpGet("{ideaId:int}/decisions")]
    public async Task<IActionResult> Decisions(int ideaId, CancellationToken ct)
        => OkData(await _screening.GetDecisionsAsync(ideaId, ct));

    [HttpPost("{ideaId:int}/decision")]
    [Authorize(Policy = Permissions.ScreeningDecide)]
    public async Task<IActionResult> Decide(int ideaId, [FromBody] ScreeningDecisionRequest request, CancellationToken ct)
        => OkResult(await _screening.DecideAsync(ideaId, request, ct));

    [HttpPost("{ideaId:int}/status")]
    [Authorize(Policy = Permissions.ScreeningDecide)]
    public async Task<IActionResult> ChangeStatus(int ideaId, [FromBody] StatusChangeRequest request, CancellationToken ct)
        => OkResult(await _screening.ChangeStatusAsync(ideaId, request, ct));
}