using YSSE.IdeaBox.Domain.Common;
using YSSE.IdeaBox.Domain.Enums;

namespace YSSE.IdeaBox.Domain.Entities;

/// <summary>Reusable item listing for sale, donation or exchange (Module I).</summary>
public class MarketplaceListing : BaseEntity, ISoftDeletable
{
    public int OwnerUserId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string Condition { get; set; } = string.Empty;

    public string? PhotoUrl { get; set; }

    public ListingType ListingType { get; set; }

    public decimal? Price { get; set; }

    public string? CoarseLocation { get; set; }

    public ListingStatus Status { get; set; } = ListingStatus.Draft;

    public DateTime? ExpiresAt { get; set; }

    public string? ModerationReason { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}

/// <summary>Buyer/recipient request to reserve a listing (FR-I05). One active at a time.</summary>
public class Reservation : BaseEntity
{
    public int MarketplaceListingId { get; set; }

    public MarketplaceListing Listing { get; set; } = null!;

    public int RequesterUserId { get; set; }

    public string? Message { get; set; }

    public ReservationStatus Status { get; set; } = ReservationStatus.Requested;
}
