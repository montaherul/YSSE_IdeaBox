using System.ComponentModel.DataAnnotations;
using YSSE.IdeaBox.Domain.Enums;

namespace YSSE.IdeaBox.Application.Dtos;

public record InquiryCreateRequest
{
    [Required] public InquiryType Type { get; init; }

    [Required, MinLength(10), MaxLength(3000)]
    public string Message { get; init; } = string.Empty;

    public string? IdempotencyKey { get; init; }
}

public record InquiryDto
{
    public int Id { get; init; }
    public int IdeaId { get; init; }
    public string IdeaTitle { get; init; } = string.Empty;
    public string SeekerName { get; init; } = string.Empty;
    public InquiryType Type { get; init; }
    public string Message { get; init; } = string.Empty;
    public bool IsDeclined { get; init; }
    public bool IsAnswered { get; init; }
    public int? ConversationId { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record InquiryRespondRequest
{
    public bool Decline { get; init; }

    [MaxLength(1000)]
    public string? Reply { get; init; }
}

public record MatchResultDto
{
    public int IdeaId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string CategoryName { get; init; } = string.Empty;
    public double Score { get; init; }
    public IReadOnlyList<string> Reasons { get; init; } = Array.Empty<string>();
}
