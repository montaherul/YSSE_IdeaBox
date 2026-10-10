using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using YSSE.IdeaBox.Application.Abstractions;
using YSSE.IdeaBox.Application.Abstractions.Services;
using YSSE.IdeaBox.Application.Dtos;

namespace YSSE.IdeaBox.Api.Controllers;

[Route("api/v1")]
public sealed class ReviewsController : ApiControllerBase
{
    private const string GuestCookieName = "ysse_guest";

    private readonly IReviewService _reviews;
    private readonly IGuestIdentityService _guestIdentity;
    private readonly IHashingService _hashing;
    private readonly ICurrentUserService _current;
    private readonly IConfiguration _config;

    public ReviewsController(
        IReviewService reviews,
        IGuestIdentityService guestIdentity,
        IHashingService hashing,
        ICurrentUserService current,
        IConfiguration config)
    {
        _reviews = reviews;
        _guestIdentity = guestIdentity;
        _hashing = hashing;
        _current = current;
        _config = config;
    }

    [HttpGet("ideas/{ideaId:int}/reviews")]
    [AllowAnonymous]
    public async Task<IActionResult> List(int ideaId, [FromQuery] int page = 1,
        [FromQuery] int pageSize = Application.Common.Pagination.DefaultPageSize, CancellationToken ct = default)
        => Paged(await _reviews.GetForIdeaAsync(ideaId, page, pageSize, ct));

    [HttpGet("ideas/{ideaId:int}/reviews/counts")]
    [AllowAnonymous]
    public async Task<IActionResult> Counts(int ideaId, CancellationToken ct)
        => OkData(await _reviews.GetCountsAsync(ideaId, ct));

    [HttpPost("ideas/{ideaId:int}/reviews")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> Create(int ideaId, [FromBody] ReviewCreateRequest request, CancellationToken ct)
        => OkResult(await _reviews.SubmitRegisteredAsync(ideaId, request, ct));

    [HttpPost("ideas/{ideaId:int}/reviews/guest")]
    [AllowAnonymous]
    [EnableRateLimiting("guest-review")]
    public async Task<IActionResult> CreateGuest(int ideaId, [FromBody] ReviewCreateRequest request, CancellationToken ct)
        => OkResult(await _reviews.SubmitGuestAsync(ideaId, request, ResolveGuestContext(), ct));

    [HttpGet("reviews/mine")]
    public async Task<IActionResult> Mine([FromQuery] int page = 1,
        [FromQuery] int pageSize = Application.Common.Pagination.DefaultPageSize, CancellationToken ct = default)
        => Paged(await _reviews.GetMineAsync(page, pageSize, ct));

    [HttpPatch("reviews/{id:int}")]
    public async Task<IActionResult> Edit(int id, [FromBody] ReviewCreateRequest request, CancellationToken ct)
        => OkResult(await _reviews.EditOwnAsync(id, request, ct));

    [HttpDelete("reviews/{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
        => OkResult(await _reviews.DeleteOwnAsync(id, ct));

    [HttpPost("reviews/{id:int}/moderate")]
    [Authorize(Policy = "Moderator")]
    public async Task<IActionResult> Moderate(int id, [FromBody] ReviewModerateRequest request, CancellationToken ct)
        => OkResult(await _reviews.ModerateAsync(id, request, ct));

    private GuestContext ResolveGuestContext()
    {
        var raw = Request.Cookies[GuestCookieName];
        if (string.IsNullOrWhiteSpace(raw))
        {
            var identity = _guestIdentity.Create();
            raw = identity.RawGuestId;
            var days = _config.GetValue<int?>("Guest:ReviewCookieDays") ?? 365;
            Response.Cookies.Append(GuestCookieName, raw, new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(days)
            });
        }

        var guestHash = _guestIdentity.Hash(raw!);
        var ipHash = _hashing.Sha256(_current.IpAddress ?? "unknown");
        return new GuestContext(raw, guestHash, ipHash);
    }
}
