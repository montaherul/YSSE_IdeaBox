using YSSE.IdeaBox.Domain.Common;
using YSSE.IdeaBox.Domain.Enums;

namespace YSSE.IdeaBox.Domain.Entities;

/// <summary>Idea content with lifecycle state (Module C).</summary>
public class Idea : BaseEntity, ISoftDeletable
{
    public string Slug { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string ShortSummary { get; set; } = string.Empty;

    public string? ProblemStatement { get; set; }

    public string? ProposedSolution { get; set; }

    public string? TargetAudience { get; set; }

    public string? ExpectedImpact { get; set; }

    public DevelopmentStage Stage { get; set; }

    public IdeaAttribution Attribution { get; set; } = IdeaAttribution.Public;

    /// <summary>Real creator is always recorded, even for anonymous ideas (FR-C10).</summary>
    public int CreatorUserId { get; set; }

    public User Creator { get; set; } = null!;

    public int CategoryId { get; set; }

    public Category Category { get; set; } = null!;

    public IdeaStatus Status { get; set; } = IdeaStatus.Draft;

    public string? SupportSought { get; set; }

    public string? SkillsNeeded { get; set; }

    public decimal? EstimatedBudget { get; set; }

    public string? ExpectedTimeline { get; set; }

    public string? SuccessMetrics { get; set; }

    public string? CollaborationPreferences { get; set; }

    public string LanguageTag { get; set; } = "en";

    public DateTime? SubmittedAt { get; set; }

    public DateTime? PublishedAt { get; set; }

    /// <summary>Editorial curation flag; featured ideas are surfaced on the public homepage.</summary>
    public bool IsFeatured { get; set; }

    public DateTime? FeaturedAt { get; set; }

    public int ViewsCount { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public ICollection<IdeaRevision> Revisions { get; set; } = new List<IdeaRevision>();

    public ICollection<IdeaAttachment> Attachments { get; set; } = new List<IdeaAttachment>();

    public ICollection<IdeaStatusHistory> StatusHistory { get; set; } = new List<IdeaStatusHistory>();

    public ICollection<IdeaScreeningDecision> ScreeningDecisions { get; set; } = new List<IdeaScreeningDecision>();

    public ICollection<IdeaTag> IdeaTags { get; set; } = new List<IdeaTag>();

    public ICollection<IdeaReview> Reviews { get; set; } = new List<IdeaReview>();

    public ICollection<IdeaVote> Votes { get; set; } = new List<IdeaVote>();

    public ICollection<IdeaComment> Comments { get; set; } = new List<IdeaComment>();

    public ICollection<IdeaFollow> Follows { get; set; } = new List<IdeaFollow>();
}

/// <summary>Material edit history (FR-C09).</summary>
public class IdeaRevision : BaseEntity
{
    public int IdeaId { get; set; }

    public Idea Idea { get; set; } = null!;

    public int RevisionNumber { get; set; }

    public string Title { get; set; } = string.Empty;

    public string ShortSummary { get; set; } = string.Empty;

    public string? ProblemStatement { get; set; }

    public string? ProposedSolution { get; set; }

    public int EditedByUserId { get; set; }

    public string? ChangeNote { get; set; }
}

/// <summary>Uploaded file metadata; content stored privately (FR-C field list).</summary>
public class IdeaAttachment : BaseEntity, ISoftDeletable
{
    public int IdeaId { get; set; }

    public Idea Idea { get; set; } = null!;

    public string FileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long SizeBytes { get; set; }

    public string StoragePath { get; set; } = string.Empty;

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }
}

/// <summary>Every status transition with actor, timestamp and reason (FR-C11).</summary>
public class IdeaStatusHistory : BaseEntity
{
    public int IdeaId { get; set; }

    public Idea Idea { get; set; } = null!;

    public IdeaStatus FromStatus { get; set; }

    public IdeaStatus ToStatus { get; set; }

    public int ActorUserId { get; set; }

    public string? Reason { get; set; }
}

/// <summary>Screening reviewer decision (FR-C05).</summary>
public class IdeaScreeningDecision : BaseEntity
{
    public int IdeaId { get; set; }

    public Idea Idea { get; set; } = null!;

    public int ReviewerUserId { get; set; }

    public IdeaStatus Decision { get; set; }

    public int? OriginalityScore { get; set; }

    public int? FeasibilityScore { get; set; }

    public int? RelevanceScore { get; set; }

    public int? ImpactScore { get; set; }

    public string? Feedback { get; set; }
}

/// <summary>Idea category, admin managed (FR-C12).</summary>
public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Idea> Ideas { get; set; } = new List<Idea>();
}

public class Tag : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public ICollection<IdeaTag> IdeaTags { get; set; } = new List<IdeaTag>();
}

public class IdeaTag : BaseEntity
{
    public int IdeaId { get; set; }

    public Idea Idea { get; set; } = null!;

    public int TagId { get; set; }

    public Tag Tag { get; set; } = null!;
}

/// <summary>Public discussion on an idea (FR-C07).</summary>
public class IdeaComment : BaseEntity, ISoftDeletable
{
    public int IdeaId { get; set; }

    public Idea Idea { get; set; } = null!;

    public int AuthorUserId { get; set; }

    public int? ParentCommentId { get; set; }

    public string Body { get; set; } = string.Empty;

    public ReviewModerationStatus ModerationStatus { get; set; } = ReviewModerationStatus.Published;

    public string? ModerationReason { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }
}

/// <summary>Bookmark/follow an idea (FR-C07).</summary>
public class IdeaFollow : BaseEntity
{
    public int IdeaId { get; set; }

    public Idea Idea { get; set; } = null!;

    public int UserId { get; set; }
}
