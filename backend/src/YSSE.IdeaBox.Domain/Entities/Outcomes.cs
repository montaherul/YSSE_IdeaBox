using YSSE.IdeaBox.Domain.Common;
using YSSE.IdeaBox.Domain.Enums;

namespace YSSE.IdeaBox.Domain.Entities;

/// <summary>
/// Reported result of a connection (Module H). Distinguishes self-reported from
/// independently verified (FR-H06). The platform never moves money (FR-H07).
/// </summary>
public class OpportunityOutcome : BaseEntity
{
    public int IdeaId { get; set; }

    public Idea Idea { get; set; } = null!;

    public int ReporterUserId { get; set; }

    public OutcomeKind Kind { get; set; }

    public OutcomeStatus Status { get; set; } = OutcomeStatus.InquiryReceived;

    public string? Description { get; set; }

    public bool IsVerified { get; set; }

    public int? VerifiedByUserId { get; set; }

    public DateTime? VerifiedAt { get; set; }

    public bool IsPublic { get; set; }
}
