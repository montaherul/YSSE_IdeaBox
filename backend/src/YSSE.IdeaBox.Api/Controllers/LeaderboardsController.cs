using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YSSE.IdeaBox.Application.Abstractions.Services;

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

    [HttpGet("idea-makers")]
    [AllowAnonymous]
    public async Task<IActionResult> IdeaMakers([FromQuery] int? votingPeriodId, [FromQuery] int take = 3, CancellationToken ct = default)
        => OkData(await _leaderboard.GetTopIdeaMakersAsync(votingPeriodId, take, ct));
}
