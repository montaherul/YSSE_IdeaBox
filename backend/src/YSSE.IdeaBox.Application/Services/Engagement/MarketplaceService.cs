using Microsoft.EntityFrameworkCore;
using YSSE.IdeaBox.Application.Abstractions;
using YSSE.IdeaBox.Application.Abstractions.Services;
using YSSE.IdeaBox.Application.Common;
using YSSE.IdeaBox.Application.Dtos;
using YSSE.IdeaBox.Domain.Entities;
using YSSE.IdeaBox.Domain.Enums;

using YSSE.IdeaBox.Application.Services.Common;

namespace YSSE.IdeaBox.Application.Services.Engagement;

public sealed class MarketplaceService : IMarketplaceService
{
    private static readonly ListingStatus[] PublicStatuses = { ListingStatus.Available, ListingStatus.Reserved };

    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _current;
    private readonly IDateTimeProvider _clock;
    private readonly IAuditWriter _audit;
    private readonly INotificationDispatcher _notifications;

    public MarketplaceService(
        IUnitOfWork db,
        ICurrentUserService current,
        IDateTimeProvider clock,
        IAuditWriter audit,
        INotificationDispatcher notifications)
    {
        _unitOfWork = db;
        _current = current;
        _clock = clock;
        _audit = audit;
        _notifications = notifications;
    }

    public async Task<PagedResult<ListingDto>> SearchAsync(ListingQuery query, CancellationToken ct = default)
    {
        var (page, pageSize) = Pagination.Normalize(query.Page, query.PageSize);
        var q = _unitOfWork.Repository<MarketplaceListing>().Query().Where(l => !l.IsDeleted);

        var privileged = _current.IsAuthenticated && (_current.IsAdmin || (query.Status is not null && _current.UserId != null));
        if (!_current.IsAdmin)
        {
            if (query.Status is null)
            {
                q = q.Where(l => PublicStatuses.Contains(l.Status));
            }
            else
            {
                q = q.Where(l => l.Status == query.Status.Value &&
                                 (l.OwnerUserId == _current.UserId || PublicStatuses.Contains(l.Status)));
            }
        }
        else if (query.Status is not null)
        {
            q = q.Where(l => l.Status == query.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var pattern = $"%{query.Search.Trim().ToLower()}%";
            q = q.Where(l => EF.Functions.Like(l.Title.ToLower(), pattern) ||
                             EF.Functions.Like(l.Description.ToLower(), pattern) ||
                             EF.Functions.Like(l.Category.ToLower(), pattern));
        }

        if (query.ListingType is not null)
        {
            q = q.Where(l => l.ListingType == query.ListingType.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Category))
        {
            q = q.Where(l => l.Category.ToLower() == query.Category.ToLower());
        }

        q = (query.Sort ?? "newest").ToLowerInvariant() switch
        {
            "oldest" => q.OrderBy(l => l.CreatedAt),
            _ => q.OrderByDescending(l => l.CreatedAt)
        };

        var total = await q.CountAsync(ct);
        var items = await q
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
.Select(l => new ListingDto
            {
                Id = l.Id,
                OwnerUserId = l.OwnerUserId,
                Title = l.Title,
                Description = l.Description,
                Category = l.Category,
                Condition = l.Condition,
                PhotoUrl = l.PhotoUrl,
                ListingType = l.ListingType,
                Price = l.Price,
                CoarseLocation = l.CoarseLocation,
                Status = l.Status,
                ActiveReservationId = l.Reservations
                    .Where(r => r.Status == ReservationStatus.Requested || r.Status == ReservationStatus.Accepted)
                    .Select(r => (int?)r.Id)
                    .FirstOrDefault(),
                ExpiresAt = l.ExpiresAt,
                CreatedAt = l.CreatedAt
            })
            .ToListAsync(ct);

        return PagedResult<ListingDto>.Create(items, page, pageSize, total);
    }

    public async Task<ListingDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var listing = await _unitOfWork.Repository<MarketplaceListing>().Query()
            .Where(l => l.Id == id && !l.IsDeleted)
.Select(l => new ListingDto
            {
                Id = l.Id,
                OwnerUserId = l.OwnerUserId,
                Title = l.Title,
                Description = l.Description,
                Category = l.Category,
                Condition = l.Condition,
                PhotoUrl = l.PhotoUrl,
                ListingType = l.ListingType,
                Price = l.Price,
                CoarseLocation = l.CoarseLocation,
                Status = l.Status,
                ActiveReservationId = l.Reservations
                    .Where(r => r.Status == ReservationStatus.Requested || r.Status == ReservationStatus.Accepted)
                    .Select(r => (int?)r.Id)
                    .FirstOrDefault(),
                ExpiresAt = l.ExpiresAt,
                CreatedAt = l.CreatedAt
            })
            .FirstOrDefaultAsync(ct);

        if (listing is null)
        {
            throw new NotFoundException("Listing not found.");
        }

        if (!PublicStatuses.Contains(listing.Status) && !_current.IsAdmin)
        {
            var isOwner = _current.UserId is not null &&
                          await _unitOfWork.Repository<MarketplaceListing>().Query().AnyAsync(l => l.Id == id && l.OwnerUserId == _current.UserId, ct);
            if (!isOwner)
            {
                throw new NotFoundException("Listing not found.");
            }
        }

        return listing;
    }

public async Task<Result<int>> CreateAsync(ListingCreateRequest request, CancellationToken ct = default)
    {
        var userId = _current.RequireUserId();
        var listing = new MarketplaceListing
        {
            OwnerUserId = userId,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Category = request.Category.Trim(),
            Condition = request.Condition.Trim(),
            PhotoUrl = request.PhotoUrl,
            ListingType = request.ListingType,
            Price = request.Price,
            CoarseLocation = request.CoarseLocation,
            Status = ListingStatus.Available,
            ExpiresAt = _clock.UtcNow.AddDays(30),
            CreatedAt = _clock.UtcNow
        };
        await _unitOfWork.Repository<MarketplaceListing>().AddAsync(listing, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result<int>.Ok(listing.Id);
    }

    public async Task<Result> UpdateAsync(int id, ListingCreateRequest request, CancellationToken ct = default)
    {
        var userId = _current.UserId;
        var listing = await _unitOfWork.Repository<MarketplaceListing>().Query().FirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted, ct);
        if (listing is null)
        {
            throw new NotFoundException("Listing not found.");
        }

        if (!_current.IsAdmin && listing.OwnerUserId != userId)
        {
            throw new ForbiddenException();
        }

        listing.Title = request.Title.Trim();
        listing.Description = request.Description.Trim();
        listing.Category = request.Category.Trim();
        listing.Condition = request.Condition.Trim();
        listing.PhotoUrl = request.PhotoUrl;
        listing.ListingType = request.ListingType;
        listing.Price = request.Price;
        listing.CoarseLocation = request.CoarseLocation;
        listing.UpdatedAt = _clock.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        if (_current.IsAuthenticated)
        {
            await _audit.RecordAsync("ListingUpdated", nameof(MarketplaceListing), listing.Id.ToString(),
                null, new { listing.Title }, ct);
        }

        return Result.Ok();
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
    {
        var userId = _current.UserId;
        var listing = await _unitOfWork.Repository<MarketplaceListing>().Query().FirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted, ct);
        if (listing is null)
        {
            throw new NotFoundException("Listing not found.");
        }

        if (!_current.IsAdmin && listing.OwnerUserId != userId)
        {
            throw new ForbiddenException();
        }

        listing.IsDeleted = true;
        listing.DeletedAt = _clock.UtcNow;
        listing.UpdatedAt = _clock.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        if (_current.IsAuthenticated)
        {
            await _audit.RecordAsync("ListingDeleted", nameof(MarketplaceListing), listing.Id.ToString(),
                null, new { listing.Title }, ct);
        }

        return Result.Ok();
    }

    public async Task<Result> UpdateStatusAsync(int id, ListingStatus status, CancellationToken ct = default)
    {
        var userId = _current.UserId;
        var listing = await _unitOfWork.Repository<MarketplaceListing>().Query().FirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted, ct);
        if (listing is null)
        {
            throw new NotFoundException("Listing not found.");
        }

        if (!_current.IsAdmin && listing.OwnerUserId != userId)
        {
            throw new ForbiddenException();
        }

        listing.Status = status;
        listing.UpdatedAt = _clock.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Ok();
    }

    public async Task<Result<int>> RequestReservationAsync(int listingId, ReservationRequest request, CancellationToken ct = default)
    {
        var userId = _current.RequireUserId();
        var listing = await _unitOfWork.Repository<MarketplaceListing>().Query().FirstOrDefaultAsync(l => l.Id == listingId && !l.IsDeleted, ct);
        if (listing is null)
        {
            throw new NotFoundException("Listing not found.");
        }

        if (listing.Status != ListingStatus.Available)
        {
            return Result<int>.Fail("This listing is not available for reservation.");
        }

        if (listing.OwnerUserId == userId)
        {
            throw new ForbiddenException("You cannot reserve your own listing.");
        }

        var active = await _unitOfWork.Repository<Reservation>().Query().AnyAsync(
            r => r.MarketplaceListingId == listingId && r.Status == ReservationStatus.Requested, ct);
        if (active)
        {
            return Result<int>.Fail("This listing already has an active reservation request.");
        }

        var reservation = new Reservation
        {
            MarketplaceListingId = listingId,
            RequesterUserId = userId,
            Message = request.Message,
            Status = ReservationStatus.Requested,
            CreatedAt = _clock.UtcNow
        };
        await _unitOfWork.Repository<Reservation>().AddAsync(reservation, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        await _notifications.NotifyAsync(listing.OwnerUserId, NotificationType.MarketplaceReservation,
            "Reservation requested", $"Someone requested your listing \"{listing.Title}\".", null, ct: ct);

        return Result<int>.Ok(reservation.Id);
    }

    public async Task<Result> RespondReservationAsync(int reservationId, bool accept, CancellationToken ct = default)
    {
        var userId = _current.UserId;
        var reservation = await _unitOfWork.Repository<Reservation>().Query().FirstOrDefaultAsync(r => r.Id == reservationId, ct);
        if (reservation is null)
        {
            throw new NotFoundException("Reservation not found.");
        }

        var listing = await _unitOfWork.Repository<MarketplaceListing>().Query().FirstOrDefaultAsync(l => l.Id == reservation.MarketplaceListingId, ct);
        if (listing is null)
        {
            throw new NotFoundException("Listing not found.");
        }

        if (!_current.IsAdmin && listing.OwnerUserId != userId)
        {
            throw new ForbiddenException();
        }

        if (reservation.Status != ReservationStatus.Requested)
        {
            return Result.Fail("This reservation has already been handled.");
        }

        if (accept)
        {
            reservation.Status = ReservationStatus.Accepted;
            listing.Status = ListingStatus.Reserved;

            var pending = await _unitOfWork.Repository<Reservation>().Query()
                .Where(r => r.MarketplaceListingId == listing.Id && r.Id != reservation.Id && r.Status == ReservationStatus.Requested)
                .ToListAsync(ct);
            foreach (var item in pending)
            {
                item.Status = ReservationStatus.Declined;
            }

            await _notifications.NotifyAsync(reservation.RequesterUserId, NotificationType.MarketplaceReservation,
                "Reservation accepted", $"Your reservation for \"{listing.Title}\" was accepted.", null, ct: ct);
        }
        else
        {
            reservation.Status = ReservationStatus.Declined;
            await _notifications.NotifyAsync(reservation.RequesterUserId, NotificationType.MarketplaceReservation,
                "Reservation declined", $"Your reservation for \"{listing.Title}\" was declined.", null, ct: ct);
        }

        reservation.UpdatedAt = _clock.UtcNow;
        listing.UpdatedAt = _clock.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Ok();
    }

    public async Task<PagedResult<ListingDto>> GetMyListingsAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var userId = _current.RequireUserId();
        var (p, s) = Pagination.Normalize(page, pageSize);

        var q = _unitOfWork.Repository<MarketplaceListing>().Query().Where(l => l.OwnerUserId == userId && !l.IsDeleted);
        var total = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(l => l.CreatedAt)
            .Skip((p - 1) * s)
            .Take(s)
.Select(l => new ListingDto
            {
                Id = l.Id,
                OwnerUserId = l.OwnerUserId,
                Title = l.Title,
                Description = l.Description,
                Category = l.Category,
                Condition = l.Condition,
                PhotoUrl = l.PhotoUrl,
                ListingType = l.ListingType,
                Price = l.Price,
                CoarseLocation = l.CoarseLocation,
                Status = l.Status,
                ActiveReservationId = l.Reservations
                    .Where(r => r.Status == ReservationStatus.Requested || r.Status == ReservationStatus.Accepted)
                    .Select(r => (int?)r.Id)
                    .FirstOrDefault(),
                ExpiresAt = l.ExpiresAt,
                CreatedAt = l.CreatedAt
            })
            .ToListAsync(ct);

        return PagedResult<ListingDto>.Create(items, p, s, total);
    }

    public async Task<IReadOnlyList<ReservationDto>> GetReservationsForListingAsync(int listingId, CancellationToken ct = default)
    {
        var userId = _current.UserId;
        var listing = await _unitOfWork.Repository<MarketplaceListing>().Query().FirstOrDefaultAsync(l => l.Id == listingId && !l.IsDeleted, ct);
        if (listing is null)
        {
            throw new NotFoundException("Listing not found.");
        }

        if (!_current.IsAdmin && listing.OwnerUserId != userId)
        {
            throw new ForbiddenException();
        }

        var reservations = await _unitOfWork.Repository<Reservation>().Query()
            .Where(r => r.MarketplaceListingId == listingId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new
            {
                r.Id,
                r.MarketplaceListingId,
                r.RequesterUserId,
                r.Message,
                r.Status,
                r.CreatedAt
            })
            .ToListAsync(ct);

        var requesterIds = reservations.Where(r => r.RequesterUserId != 0).Select(r => r.RequesterUserId).Distinct().ToList();
        var requesterNames = await _unitOfWork.Repository<User>().Query()
            .Where(u => requesterIds.Contains(u.Id))
            .Select(u => new { u.Id, u.DisplayName })
            .ToDictionaryAsync(u => u.Id, u => u.DisplayName, ct);

        return reservations
            .Select(r => new ReservationDto
            {
                Id = r.Id,
                MarketplaceListingId = r.MarketplaceListingId,
                RequesterName = requesterNames.TryGetValue(r.RequesterUserId, out var name) ? name : "Member",
                Message = r.Message,
                Status = r.Status,
                CreatedAt = r.CreatedAt
            })
            .ToList();
    }
}


