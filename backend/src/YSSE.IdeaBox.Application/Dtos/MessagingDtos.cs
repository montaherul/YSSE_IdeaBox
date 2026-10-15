using System.ComponentModel.DataAnnotations;
using YSSE.IdeaBox.Domain.Enums;

namespace YSSE.IdeaBox.Application.Dtos;

public record ConversationListItemDto
{
    public int Id { get; init; }
    public int? IdeaId { get; init; }
    public string? IdeaTitle { get; init; }
    public ConversationStatus Status { get; init; }
    public string CounterpartyName { get; init; } = string.Empty;
    public string? LastMessagePreview { get; init; }
    public DateTime? LastMessageAt { get; init; }
    public int UnreadCount { get; init; }
}

public record ConversationDetailDto
{
    public int Id { get; init; }
    public int? IdeaId { get; init; }
    public ConversationStatus Status { get; init; }
    public IReadOnlyList<MessageDto> Messages { get; init; } = Array.Empty<MessageDto>();
}

public record MessageDto
{
    public int Id { get; init; }
    public int SenderUserId { get; init; }
    public string SenderName { get; init; } = string.Empty;
    public bool IsMine { get; init; }
    public string Body { get; init; } = string.Empty;
    public DateTime? DeliveredAt { get; init; }
    public DateTime? ReadAt { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record MessageSendRequest
{
    [Required, MinLength(1), MaxLength(4000)]
    public string Body { get; init; } = string.Empty;

    public string? IdempotencyKey { get; init; }
}

public record MessageUpdateRequest
{
    [Required, MinLength(1), MaxLength(4000)]
    public string Body { get; init; } = string.Empty;
}

public record StartConversationRequest([Required] int IdeaId);

public record ReportCreateRequest
{
    [Required] public ReportTargetType TargetType { get; init; }
    [Required] public int TargetId { get; init; }
    [Required, MaxLength(500)] public string Reason { get; init; } = string.Empty;
    [MaxLength(2000)] public string? Details { get; init; }
}

public record ReportDto
{
    public int Id { get; init; }
    public ReportTargetType TargetType { get; init; }
    public int TargetId { get; init; }
    public string Reason { get; init; } = string.Empty;
    public string? Details { get; init; }
    public ReportStatus Status { get; init; }
    public string ReporterName { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? ResolvedAt { get; init; }
    public string? Resolution { get; init; }
}

public record ReportResolveRequest
{
    [Required] public ReportStatus Status { get; init; }
    [MaxLength(1000)] public string? Resolution { get; init; }
}

public record ReportUpdateRequest
{
    [Required, MaxLength(500)] public string Reason { get; init; } = string.Empty;
    [MaxLength(2000)] public string? Details { get; init; }
    [Required] public ReportStatus Status { get; init; }
}
