using System.ComponentModel.DataAnnotations;
using YSSE.IdeaBox.Domain.Enums;

namespace YSSE.IdeaBox.Application.Dtos;

public record ReviewDto
{
    public int Id { get; init; }
    public string Body { get; init; } = string.Empty;
    public int? Rating { get; init; }
    public bool IsGuest { get; init; }
    public ReviewModerationStatus ModerationStatus { get; init; }
    public string DisplayName { get; init; } = "Anonymous";
    public DateTime CreatedAt { get; init; }
    public bool CanEdit { get; init; }
}

public record MyReviewDto
{
    public int Id { get; init; }
    public int IdeaId { get; init; }
    public string IdeaSlug { get; init; } = string.Empty;
    public string IdeaTitle { get; init; } = string.Empty;
    public IdeaStatus Status { get; init; }
    public string Body { get; init; } = string.Empty;
    public int? Rating { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record ReviewCountsDto
{
    public int AnonymousPublished { get; init; }
    public int RegisteredPublished { get; init; }
    public int TotalPublished => AnonymousPublished + RegisteredPublished;
    public double? AverageRating { get; init; }
}

public record ReviewCreateRequest
{
    [Required, MinLength(10), MaxLength(3000)]
    public string Body { get; init; } = string.Empty;

    [Range(1, 5)]
    public int? Rating { get; init; }
}

public record ReviewModerateRequest
{
    [Required]
    public ReviewModerationStatus Status { get; init; }

    [MaxLength(500)]
    public string? Reason { get; init; }
}
