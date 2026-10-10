using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YSSE.IdeaBox.Api.Requests;
using YSSE.IdeaBox.Application.Abstractions.Services;
using YSSE.IdeaBox.Application.Common;
using YSSE.IdeaBox.Application.Dtos;

namespace YSSE.IdeaBox.Api.Controllers;

[Route("api/v1")]
public sealed class MarketplaceController : ApiControllerBase
{
    private readonly IMarketplaceService _marketplace;

    public MarketplaceController(IMarketplaceService marketplace) => _marketplace = marketplace;

    [HttpGet("marketplace")]
    [AllowAnonymous]
    public async Task<IActionResult> Search([FromQuery] ListingQuery query, CancellationToken ct)
        => Paged(await _marketplace.SearchAsync(query, ct));

    [HttpGet("marketplace/mine")]
    public async Task<IActionResult> Mine([FromQuery] int page = 1,
        [FromQuery] int pageSize = Pagination.DefaultPageSize, CancellationToken ct = default)
        => Paged(await _marketplace.GetMyListingsAsync(page, pageSize, ct));

    [HttpGet("marketplace/{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var listing = await _marketplace.GetByIdAsync(id, ct);
        return listing is null ? NotFoundError("Listing not found.") : OkData(listing);
    }

    [HttpPost("marketplace")]
    public async Task<IActionResult> Create([FromBody] ListingCreateRequest request, CancellationToken ct)
        => OkResult(await _marketplace.CreateAsync(request, ct));

    [HttpPost("marketplace/{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] ListingStatusRequest request, CancellationToken ct)
        => OkResult(await _marketplace.UpdateStatusAsync(id, request.Status, ct));

    [HttpPost("marketplace/{listingId:int}/reservations")]
    public async Task<IActionResult> RequestReservation(int listingId, [FromBody] ReservationRequest request, CancellationToken ct)
        => OkResult(await _marketplace.RequestReservationAsync(listingId, request, ct));

    [HttpGet("marketplace/{listingId:int}/reservations")]
    public async Task<IActionResult> Reservations(int listingId, CancellationToken ct)
        => OkData(await _marketplace.GetReservationsForListingAsync(listingId, ct));

    [HttpPost("reservations/{id:int}/respond")]
    public async Task<IActionResult> RespondReservation(int id, [FromBody] ReservationRespondRequest request, CancellationToken ct)
        => OkResult(await _marketplace.RespondReservationAsync(id, request.Accept, ct));
}
