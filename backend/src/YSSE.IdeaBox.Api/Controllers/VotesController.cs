using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YSSE.IdeaBox.Application.Abstractions.Services;
using YSSE.IdeaBox.Application.Dtos;

namespace YSSE.IdeaBox.Api.Controllers;

[Route("api/v1")]
public sealed class VotesController : ApiControllerBase
{
    private readonly IVotingService _voting;

    public VotesController(IVotingService voting) => _voting = voting;

    [HttpGet("ideas/{ideaId:int}/votes")]
    [AllowAnonymous]
    public async Task<IActionResult> State(int ideaId, CancellationToken ct)
        => OkData(await _voting.GetVoteStateAsync(ideaId, ct));

    [HttpPost("ideas/{ideaId:int}/votes")]
    public async Task<IActionResult> Cast(int ideaId, CancellationToken ct)
        => OkResult(await _voting.CastAsync(ideaId, ct));

    [HttpDelete("ideas/{ideaId:int}/votes")]
    public async Task<IActionResult> Remove(int ideaId, CancellationToken ct)
        => OkResult(await _voting.RemoveAsync(ideaId, ct));

    [HttpGet("voting-periods")]
    [AllowAnonymous]
    public async Task<IActionResult> Periods(CancellationToken ct)
        => OkData(await _voting.GetPeriodsAsync(ct));

    [HttpPost("voting-periods")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> CreatePeriod([FromBody] VotingPeriodCreateRequest request, CancellationToken ct)
        => OkResult(await _voting.CreatePeriodAsync(request, ct));

    [HttpPost("voting-periods/{id:int}/activate")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> Activate(int id, CancellationToken ct)
        => OkResult(await _voting.SetPeriodActiveAsync(id, true, ct));

    [HttpPost("voting-periods/{id:int}/close")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> Close(int id, CancellationToken ct)
        => OkResult(await _voting.ClosePeriodAsync(id, ct));
}
