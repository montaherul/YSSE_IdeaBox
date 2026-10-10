using YSSE.IdeaBox.Domain.Common;
using YSSE.IdeaBox.Domain.Enums;

namespace YSSE.IdeaBox.Domain.Entities;

/// <summary>
/// Community review — feedback, never a vote (Module D). Either a registered
/// member (ReviewerUserId) or a guest (GuestVisitorId) is set, never both.
/// </summary>
public class IdeaReview : BaseEntity, ISoftDeletable
{
    public int IdeaId { get; set; }

    public Idea Idea { get; set; } = null!;

    public int? ReviewerUserId { get; set; }

    public int? GuestVisitorId { get; set; }

    public GuestVisitor? GuestVisitor { get; set; }

    public string Body { get; set; } = string.Empty;

    public int? Rating { get; set; }

    public ReviewModerationStatus ModerationStatus { get; set; } = ReviewModerationStatus.Published;

    public string? ModerationReason { get; set; }

    public bool IsGuest { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }
}

/// <summary>
/// Opaque guest record enabling the best-effort one-review-one-idea limit (FR-D02/FR-D10).
/// Stores a hash of the guest ID, never personal data.
/// </summary>
public class GuestVisitor : BaseEntity
{
    /// <summary>SHA-256 hash of the opaque 128-bit+ guest identifier.</summary>
    public string GuestIdHash { get; set; } = string.Empty;

    /// <summary>Hash of coarse IP-derived data, short-lived (FR-D/8.5).</summary>
    public string? IpHash { get; set; }

    public int? FirstReviewIdeaId { get; set; }

    public DateTime FirstSeenAt { get; set; } = DateTime.UtcNow;

    public DateTime LastSeenAt { get; set; } = DateTime.UtcNow;
}
