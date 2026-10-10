using YSSE.IdeaBox.Domain.Common;
using YSSE.IdeaBox.Domain.Enums;

namespace YSSE.IdeaBox.Domain.Entities;

/// <summary>Structured private expression of interest in a published idea (Module F).</summary>
public class EntrepreneurInquiry : BaseEntity
{
    public int IdeaId { get; set; }

    public Idea Idea { get; set; } = null!;

    public int SeekerUserId { get; set; }

    public User Seeker { get; set; } = null!;

    public InquiryType Type { get; set; }

    public string Message { get; set; } = string.Empty;

    /// <summary>Idempotency key so retries do not create duplicate inquiries (Section 21).</summary>
    public string? IdempotencyKey { get; set; }

    public bool IsDeclined { get; set; }

    public bool IsReported { get; set; }

    public bool IsAnswered { get; set; }

    public DateTime? AnsweredAt { get; set; }

    public int? ConversationId { get; set; }

    public Conversation? Conversation { get; set; }
}

/// <summary>Private saved-ideas list for opportunity seekers (FR-F07).</summary>
public class WatchlistItem : BaseEntity
{
    public int UserId { get; set; }

    public int IdeaId { get; set; }

    public Idea Idea { get; set; } = null!;
}
