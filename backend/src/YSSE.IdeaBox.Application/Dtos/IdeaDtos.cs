using System.ComponentModel.DataAnnotations;
using YSSE.IdeaBox.Domain.Enums;

namespace YSSE.IdeaBox.Application.Dtos;

public record IdeaListQuery
{
    public string? Search { get; init; }
    public int? CategoryId { get; init; }
    public string? Tag { get; init; }
    public DevelopmentStage? Stage { get; init; }
    public IdeaStatus? Status { get; init; }
    public bool? Featured { get; init; }
    public string? Sort { get; init; }
    public int? Page { get; init; }
    public int? PageSize { get; init; }
}

public record IdeaListItemDto
{
    public int Id { get; init; }
    public string Slug { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string ShortSummary { get; init; } = string.Empty;
    public string CategoryName { get; init; } = string.Empty;
    public DevelopmentStage Stage { get; init; }
    public IdeaStatus Status { get; init; }
    public bool IsFeatured { get; init; }
    public bool IsAnonymous { get; init; }
    public string? CreatorAlias { get; init; }
    public int RegisteredReviewCount { get; init; }
    public int GuestReviewCount { get; init; }
    public int VoteCount { get; init; }
    public DateTime? PublishedAt { get; init; }
    public string? LanguageTag { get; init; }
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
}

public record IdeaDetailDto : IdeaListItemDto
{
    public string? ProblemStatement { get; init; }
    public string? ProposedSolution { get; init; }
    public string? TargetAudience { get; init; }
    public string? ExpectedImpact { get; init; }
    public string? SupportSought { get; init; }
    public string? SkillsNeeded { get; init; }
    public decimal? EstimatedBudget { get; init; }
    public string? ExpectedTimeline { get; init; }
    public string? SuccessMetrics { get; init; }
    public string? CollaborationPreferences { get; init; }
    public int ViewsCount { get; init; }
    public bool CanEdit { get; init; }
    public IReadOnlyList<IdeaAttachmentDto> Attachments { get; init; } = Array.Empty<IdeaAttachmentDto>();
    public IReadOnlyList<IdeaStatusHistoryDto> StatusHistory { get; init; } = Array.Empty<IdeaStatusHistoryDto>();
}

public record IdeaAttachmentDto
{
    public int Id { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long SizeBytes { get; init; }
}

/// <summary>Authorized stream of a private idea attachment for download.</summary>
public record IdeaAttachmentDownload(string FileName, string ContentType, Stream Content);

public record IdeaStatusHistoryDto
{
    public IdeaStatus FromStatus { get; init; }
    public IdeaStatus ToStatus { get; init; }
    public string? Reason { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record IdeaCreateRequest
{
    [Required, MaxLength(120)]
    public string Title { get; init; } = string.Empty;

    [Required, MaxLength(300)]
    public string ShortSummary { get; init; } = string.Empty;

    [Required, MaxLength(5000)]
    public string ProblemStatement { get; init; } = string.Empty;

    [Required, MaxLength(5000)]
    public string ProposedSolution { get; init; } = string.Empty;

    [Required, MaxLength(500)]
    public string TargetAudience { get; init; } = string.Empty;

    [Required, MaxLength(500)]
    public string ExpectedImpact { get; init; } = string.Empty;

    [Required]
    public DevelopmentStage Stage { get; init; }

    [Required]
    public IdeaAttribution Attribution { get; init; }

    public int CategoryId { get; init; }

    [MaxLength(500)]
    public string? SupportSought { get; init; }

    [MaxLength(500)]
    public string? SkillsNeeded { get; init; }

    public decimal? EstimatedBudget { get; init; }

    [MaxLength(200)]
    public string? ExpectedTimeline { get; init; }

    [MaxLength(1000)]
    public string? SuccessMetrics { get; init; }

    [MaxLength(500)]
    public string? CollaborationPreferences { get; init; }

    [MaxLength(8)]
    public string LanguageTag { get; init; } = "en";

    public List<string>? Tags { get; init; }

    /// <summary>Quick-idea mode omits the long fields (FR-B28).</summary>
    public bool IsQuickIdea { get; init; }
}

public record IdeaUpdateRequest : IdeaCreateRequest
{
    [MaxLength(500)]
    public string? ChangeNote { get; init; }
}

public record QuickIdeaRequest
{
    [Required, MaxLength(120)] public string Title { get; init; } = string.Empty;
    [Required, MaxLength(5000)] public string ProblemStatement { get; init; } = string.Empty;
    [Required, MaxLength(5000)] public string ProposedSolution { get; init; } = string.Empty;
    public int? CategoryId { get; init; }
    public string LanguageTag { get; init; } = "en";
    public IdeaAttribution Attribution { get; init; } = IdeaAttribution.Public;
}

public record StatusChangeRequest
{
    [Required]
    public IdeaStatus ToStatus { get; init; }

    [MaxLength(1000)]
    public string? Reason { get; init; }
}

public record ScreeningDecisionRequest
{
    [Required]
    public string Decision { get; init; } = string.Empty;

    [Range(1, 5)] public int? OriginalityScore { get; init; }
    [Range(1, 5)] public int? FeasibilityScore { get; init; }
    [Range(1, 5)] public int? RelevanceScore { get; init; }
    [Range(1, 5)] public int? ImpactScore { get; init; }

    [MaxLength(2000)]
    public string? Feedback { get; init; }
}
