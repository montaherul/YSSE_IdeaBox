using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YSSE.IdeaBox.Application.Abstractions.Services;
using YSSE.IdeaBox.Application.Common;
using YSSE.IdeaBox.Application.Dtos;

namespace YSSE.IdeaBox.Api.Controllers;

[Route("api/v1/leaderboards")]
public sealed class LeaderboardsController : ApiControllerBase
{
    private readonly ILeaderboardService _leaderboard;

    public LeaderboardsController(ILeaderboardService leaderboard) => _leaderboard = leaderboard;

    [HttpGet("ideas")]
    [AllowAnonymous]
    public async Task<IActionResult> Ideas([FromQuery] int? votingPeriodId, [FromQuery] int take = 3, CancellationToken ct = default)
        => OkData(await _leaderboard.GetTopIdeasAsync(votingPeriodId, take, ct));

    [HttpGet("ideas/current-month")]
    [AllowAnonymous]
    public async Task<IActionResult> MonthlyTop([FromQuery] int take = 3, CancellationToken ct = default)
        => OkData(await _leaderboard.GetMonthlyTopIdeasAsync(take, ct));

    [HttpPost("ideas/current-month/announce")]
    [Authorize(Policy = Permissions.VotingPeriodsManage)]
    public async Task<IActionResult> Announce([FromBody] MonthlyAnnounceRequest? request, CancellationToken ct = default)
        => OkData(await _leaderboard.AnnounceMonthlyTopAsync(request?.Message, ct));

    [HttpGet("idea-makers")]
    [AllowAnonymous]
    public async Task<IActionResult> IdeaMakers([FromQuery] int? votingPeriodId, [FromQuery] int take = 3, CancellationToken ct = default)
        => OkData(await _leaderboard.GetTopIdeaMakersAsync(votingPeriodId, take, ct));
}
