using YSSE.IdeaBox.Domain.Common;
using YSSE.IdeaBox.Domain.Enums;

namespace YSSE.IdeaBox.Domain.Entities;

/// <summary>Private conversation between an opportunity seeker and a creator (Module G).</summary>
public class Conversation : BaseEntity
{
    public int? IdeaId { get; set; }

    public ConversationStatus Status { get; set; } = ConversationStatus.Open;

    public DateTime? LastMessageAt { get; set; }

    public ICollection<ConversationParticipant> Participants { get; set; } = new List<ConversationParticipant>();

    public ICollection<Message> Messages { get; set; } = new List<Message>();
}

/// <summary>Membership row; every read/write requires a participant row (Section 20).</summary>
public class ConversationParticipant : BaseEntity
{
    public int ConversationId { get; set; }

    public Conversation Conversation { get; set; } = null!;

    public int UserId { get; set; }

    public bool IsMuted { get; set; }

    public DateTime? LastReadAt { get; set; }
}

public class Message : BaseEntity, ISoftDeletable
{
    public int ConversationId { get; set; }

    public Conversation Conversation { get; set; } = null!;

    public int SenderUserId { get; set; }

    public string Body { get; set; } = string.Empty;

    public DateTime? DeliveredAt { get; set; }

    public DateTime? ReadAt { get; set; }

    public string? IdempotencyKey { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public ICollection<MessageAttachment> Attachments { get; set; } = new List<MessageAttachment>();
}

public class MessageAttachment : BaseEntity
{
    public int MessageId { get; set; }

    public Message Message { get; set; } = null!;

    public string FileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long SizeBytes { get; set; }

    public string StoragePath { get; set; } = string.Empty;
}

/// <summary>A user blocking another; blocks cannot be bypassed (FR-G08).</summary>
public class UserBlock : BaseEntity
{
    public int BlockerUserId { get; set; }

    public int BlockedUserId { get; set; }

    public string? Reason { get; set; }
}
