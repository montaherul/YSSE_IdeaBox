using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using YSSE.IdeaBox.Application.Abstractions.Services;
using YSSE.IdeaBox.Application.Common;
using YSSE.IdeaBox.Application.Dtos;

namespace YSSE.IdeaBox.Api.Controllers;

[Route("api/v1")]
public sealed class InquiriesController : ApiControllerBase
{
    private readonly IInquiryService _inquiries;

    public InquiriesController(IInquiryService inquiries) => _inquiries = inquiries;

    [HttpPost("ideas/{ideaId:int}/inquiries")]
    [Authorize(Policy = "OpportunitySeeker")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> Send(int ideaId, [FromBody] InquiryCreateRequest request, CancellationToken ct)
        => OkResult(await _inquiries.SendAsync(ideaId, request, ct));

    [HttpGet("inquiries/received")]
    public async Task<IActionResult> Received([FromQuery] int page = 1,
        [FromQuery] int pageSize = Pagination.DefaultPageSize, CancellationToken ct = default)
        => Paged(await _inquiries.GetReceivedAsync(page, pageSize, ct));

    [HttpGet("inquiries/sent")]
    public async Task<IActionResult> Sent([FromQuery] int page = 1,
        [FromQuery] int pageSize = Pagination.DefaultPageSize, CancellationToken ct = default)
        => Paged(await _inquiries.GetSentAsync(page, pageSize, ct));

    [HttpPost("inquiries/{id:int}/respond")]
    public async Task<IActionResult> Respond(int id, [FromBody] InquiryRespondRequest request, CancellationToken ct)
        => OkResult(await _inquiries.RespondAsync(id, request, ct));

    [HttpGet("watchlist")]
    public async Task<IActionResult> Watchlist([FromQuery] int page = 1,
        [FromQuery] int pageSize = Pagination.DefaultPageSize, CancellationToken ct = default)
        => Paged(await _inquiries.GetWatchlistAsync(page, pageSize, ct));

    [HttpPost("watchlist/{ideaId:int}")]
    public async Task<IActionResult> AddWatchlist(int ideaId, CancellationToken ct)
        => OkResult(await _inquiries.AddWatchlistAsync(ideaId, ct));

    [HttpDelete("watchlist/{ideaId:int}")]
    public async Task<IActionResult> RemoveWatchlist(int ideaId, CancellationToken ct)
        => OkResult(await _inquiries.RemoveWatchlistAsync(ideaId, ct));

    [HttpGet("matches")]
    [Authorize(Policy = "OpportunitySeeker")]
    public async Task<IActionResult> Matches([FromQuery] int take = 10, CancellationToken ct = default)
        => OkData(await _inquiries.GetMatchesAsync(take, ct));
}
