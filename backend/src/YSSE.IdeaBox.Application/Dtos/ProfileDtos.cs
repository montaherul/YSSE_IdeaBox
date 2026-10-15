using System.ComponentModel.DataAnnotations;
using YSSE.IdeaBox.Domain.Enums;

namespace YSSE.IdeaBox.Application.Dtos;

public record MemberProfileDto
{
    public string DisplayName { get; init; } = string.Empty;
    public string? Biography { get; init; }
    public string? Skills { get; init; }
    public string? Interests { get; init; }
    public string? ImageUrl { get; init; }
    public string PreferredLanguage { get; init; } = "en";
}

public record MemberProfileUpdateRequest
{
    [Required, MaxLength(120)]
    public string DisplayName { get; init; } = string.Empty;

    [MaxLength(1000)]
    public string? Biography { get; init; }

    [MaxLength(500)]
    public string? Skills { get; init; }

    [MaxLength(500)]
    public string? Interests { get; init; }

    [MaxLength(8)]
    public string PreferredLanguage { get; init; } = "en";
}

public record CreatorProfileDto
{
    public int Id { get; init; }
    public int UserId { get; init; }
    public string Alias { get; init; } = string.Empty;
    public string? Headline { get; init; }
    public string? ShortBio { get; init; }
    public string? District { get; init; }
    public string? Languages { get; init; }
    public string? SkillsOffered { get; init; }
    public string? HelpNeeded { get; init; }
    public string? AreasOfInterest { get; init; }
    public string? Links { get; init; }
    public string? Availability { get; init; }
    public string? PhotoUrl { get; init; }
    public int PublishedIdeaCount { get; init; }
    public IReadOnlyList<ProfileVisibilityDto> FieldVisibilities { get; init; } = Array.Empty<ProfileVisibilityDto>();
}

public record ProfileVisibilityDto(string FieldName, ProfileVisibility Visibility);

public record CreatorProfileUpdateRequest
{
    [Required, MaxLength(60)]
    public string Alias { get; init; } = string.Empty;

    [MaxLength(160)]
    public string? Headline { get; init; }

    [MaxLength(1000)]
    public string? ShortBio { get; init; }

    [MaxLength(120)]
    public string? District { get; init; }

    [MaxLength(200)]
    public string? Languages { get; init; }

    [MaxLength(500)]
    public string? SkillsOffered { get; init; }

    [MaxLength(500)]
    public string? HelpNeeded { get; init; }

    [MaxLength(500)]
    public string? AreasOfInterest { get; init; }

    [MaxLength(500)]
    public string? Links { get; init; }

    [MaxLength(120)]
    public string? Availability { get; init; }

    [MaxLength(500)]
    public string? PhotoUrl { get; init; }

    public List<ProfileVisibilityDto>? FieldVisibilities { get; init; }
}

public record EntrepreneurProfileDto
{
    public int Id { get; init; }
    public OpportunitySeekerKind Kind { get; init; }
    public string Organization { get; init; } = string.Empty;
    public string Industry { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Website { get; init; }
    public string? CoarseLocation { get; init; }
    public string? InterestAreas { get; init; }
    public string? Expertise { get; init; }
    public string? PreferredCollaborationTypes { get; init; }
    public bool IsVerified { get; init; }
    public bool IsApproved { get; init; }
}

public record EntrepreneurProfileUpdateRequest
{
    [Required]
    public OpportunitySeekerKind Kind { get; init; }

    [Required, MaxLength(200)]
    public string Organization { get; init; } = string.Empty;

    [Required, MaxLength(150)]
    public string Industry { get; init; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; init; }

    [MaxLength(300)]
    public string? Website { get; init; }

    [MaxLength(150)]
    public string? CoarseLocation { get; init; }

    [MaxLength(500)]
    public string? InterestAreas { get; init; }

    [MaxLength(500)]
    public string? Expertise { get; init; }

    [MaxLength(300)]
    public string? PreferredCollaborationTypes { get; init; }
}
