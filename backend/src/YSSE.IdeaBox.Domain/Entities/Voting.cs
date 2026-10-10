using YSSE.IdeaBox.Domain.Common;
using YSSE.IdeaBox.Domain.Enums;

namespace YSSE.IdeaBox.Domain.Entities;

/// <summary>Official ranking signal, authenticated + eligible members only (Module E).</summary>
public class IdeaVote : BaseEntity, ISoftDeletable
{
    public int IdeaId { get; set; }

    public Idea Idea { get; set; } = null!;

    public int UserId { get; set; }

    public int VotingPeriodId { get; set; }

    public VotingPeriod VotingPeriod { get; set; } = null!;

    /// <summary>False when the vote is ruled ineligible/fraudulent and excluded from rankings (FR-E08).</summary>
    public bool IsCounted { get; set; } = true;

    public string? ExclusionReason { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }
}

/// <summary>Configurable voting window (FR-E06).</summary>
public class VotingPeriod : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public DateTime StartAt { get; set; }

    public DateTime EndAt { get; set; }

    public VoteVisibility Visibility { get; set; } = VoteVisibility.HiddenUntilClose;

    public int MinimumAccountAgeDays { get; set; } = 7;

    public bool RequiresVerifiedAccount { get; set; } = true;

    public bool IsActive { get; set; }

    public bool IsClosed { get; set; }

    public ICollection<IdeaVote> Votes { get; set; } = new List<IdeaVote>();
}
