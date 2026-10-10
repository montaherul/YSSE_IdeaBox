using YSSE.IdeaBox.Domain.Common;
using YSSE.IdeaBox.Domain.Enums;

namespace YSSE.IdeaBox.Domain.Entities;

/// <summary>Base member profile (FR-B03).</summary>
public class MemberProfile : BaseEntity
{
    public int UserId { get; set; }

    public User User { get; set; } = null!;

    public string DisplayName { get; set; } = string.Empty;

    public string? Biography { get; set; }

    public string? Skills { get; set; }

    public string? Interests { get; set; }

    public string? ImageUrl { get; set; }

    public string PreferredLanguage { get; set; } = "en";
}

/// <summary>
/// Public-facing creator page (FR-B13..FR-B23). The alias is separate from the
/// real name and anonymous ideas are never listed here (FR-B17).
/// </summary>
public class CreatorProfile : BaseEntity, ISoftDeletable
{
    public int UserId { get; set; }

    public User User { get; set; } = null!;

    public string Alias { get; set; } = string.Empty;

    public string? Headline { get; set; }

    public string? ShortBio { get; set; }

    public string? District { get; set; }

    public string? Languages { get; set; }

    public string? SkillsOffered { get; set; }

    public string? HelpNeeded { get; set; }

    public string? AreasOfInterest { get; set; }

    public string? Links { get; set; }

    public string? Availability { get; set; }

    public string? PhotoUrl { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public ICollection<ProfileFieldVisibility> FieldVisibilities { get; set; } = new List<ProfileFieldVisibility>();
}

/// <summary>Per-field visibility setting (FR-B16).</summary>
public class ProfileFieldVisibility : BaseEntity
{
    public int CreatorProfileId { get; set; }

    public CreatorProfile CreatorProfile { get; set; } = null!;

    public string FieldName { get; set; } = string.Empty;

    public ProfileVisibility Visibility { get; set; } = ProfileVisibility.Public;
}

/// <summary>
/// Professional profile for entrepreneurs / investors / mentors (FR-B04, FR-F04).
/// Distinguishes self-declared vs verified business info (FR-B06).
/// </summary>
public class EntrepreneurProfile : BaseEntity
{
    public int UserId { get; set; }

    public User User { get; set; } = null!;

    public OpportunitySeekerKind Kind { get; set; }

    public string Organization { get; set; } = string.Empty;

    public string Industry { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Website { get; set; }

    public string? CoarseLocation { get; set; }

    public string? InterestAreas { get; set; }

    public string? Expertise { get; set; }

    public string? PreferredCollaborationTypes { get; set; }

    public bool IsVerified { get; set; }

    public bool IsApproved { get; set; }

    public DateTime? ApprovedAt { get; set; }
}
