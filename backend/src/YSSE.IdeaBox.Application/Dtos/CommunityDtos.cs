using System.ComponentModel.DataAnnotations;
using YSSE.IdeaBox.Domain.Enums;

namespace YSSE.IdeaBox.Application.Dtos;

public record CommentDto
{
    public int Id { get; init; }
    public int IdeaId { get; init; }
    public int? ParentCommentId { get; init; }
    public string Body { get; init; } = string.Empty;
    public string AuthorName { get; init; } = string.Empty;
    public ReviewModerationStatus ModerationStatus { get; init; }
    public bool CanDelete { get; init; }
    public bool CanEdit { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record CommentCreateRequest
{
    [Required, MinLength(1), MaxLength(2000)]
    public string Body { get; init; } = string.Empty;

    public int? ParentCommentId { get; init; }
}

public record CommentUpdateRequest
{
    [Required, MinLength(1), MaxLength(2000)]
    public string Body { get; init; } = string.Empty;
}

public record ProblemPostDto
{
    public int Id { get; init; }
    public int AuthorUserId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string WhoAffected { get; init; } = string.Empty;
    public string? CoarseLocation { get; init; }
    public string WhyItMatters { get; init; } = string.Empty;
    public string? Urgency { get; init; }
    public string? PhotoUrl { get; init; }
    public ProblemStatus Status { get; init; }
    public int EndorsementCount { get; init; }
    public bool EndorsedByMe { get; init; }
    public int LinkedIdeaCount { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record ProblemPostCreateRequest
{
    [Required, MaxLength(150)] public string Title { get; init; } = string.Empty;
    [Required, MaxLength(1000)] public string WhoAffected { get; init; } = string.Empty;
    [MaxLength(150)] public string? CoarseLocation { get; init; }
    [Required, MaxLength(3000)] public string WhyItMatters { get; init; } = string.Empty;
    [MaxLength(50)] public string? Urgency { get; init; }
    [MaxLength(500)] public string? PhotoUrl { get; init; }
}

public record ProblemStatusChangeRequest
{
    [Required] public ProblemStatus Status { get; init; }
    [MaxLength(500)] public string? Reason { get; init; }
}

public record LinkIdeaToProblemRequest([Required] int IdeaId);

public record LookupDto(int Id, string Name);

public record CategoryDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsActive { get; init; }
}

public record CategoryUpsertRequest
{
    [Required, MaxLength(120)] public string Name { get; init; } = string.Empty;
    [MaxLength(120)] public string? Slug { get; init; }
    [MaxLength(500)] public string? Description { get; init; }
    public bool IsActive { get; init; } = true;
}

public record TagDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public int UsageCount { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record TagUpsertRequest
{
    [Required, MaxLength(120)] public string Name { get; init; } = string.Empty;
    [MaxLength(120)] public string? Slug { get; init; }
}
