using YSSE.IdeaBox.Domain.Common;
using YSSE.IdeaBox.Domain.Enums;

namespace YSSE.IdeaBox.Domain.Entities;

/// <summary>Problem raised without a solution (FR-C14).</summary>
public class ProblemPost : BaseEntity, ISoftDeletable
{
    public int AuthorUserId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string WhoAffected { get; set; } = string.Empty;

    public string? CoarseLocation { get; set; }

    public string WhyItMatters { get; set; } = string.Empty;

    public string? Urgency { get; set; }

    public string? PhotoUrl { get; set; }

    public ProblemStatus Status { get; set; } = ProblemStatus.Open;

    public string Attribution { get; set; } = "Public";

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public ICollection<ProblemEndorsement> Endorsements { get; set; } = new List<ProblemEndorsement>();

    public ICollection<IdeaProblemLink> IdeaLinks { get; set; } = new List<IdeaProblemLink>();
}

/// <summary>"Me too" endorsement; members-only, one per member, never affects voting (FR-C17).</summary>
public class ProblemEndorsement : BaseEntity
{
    public int ProblemPostId { get; set; }

    public ProblemPost ProblemPost { get; set; } = null!;

    public int UserId { get; set; }
}

/// <summary>Links a "Solve this" idea to a problem (FR-C15).</summary>
public class IdeaProblemLink : BaseEntity
{
    public int IdeaId { get; set; }

    public Idea Idea { get; set; } = null!;

    public int ProblemPostId { get; set; }

    public ProblemPost ProblemPost { get; set; } = null!;
}
