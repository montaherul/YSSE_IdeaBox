using System.ComponentModel.DataAnnotations;
using YSSE.IdeaBox.Domain.Enums;

namespace YSSE.IdeaBox.Application.Dtos;

public record ListingQuery
{
    public string? Search { get; init; }
    public ListingType? ListingType { get; init; }
    public string? Category { get; init; }
    public ListingStatus? Status { get; init; }
    public string? Sort { get; init; }
    public int? Page { get; init; }
    public int? PageSize { get; init; }
}

public record AdminUserQuery
{
    public string? Search { get; init; }
    public string? Role { get; init; }
    public bool? IsSuspended { get; init; }
    public int? Page { get; init; }
    public int? PageSize { get; init; }
}

public record ProblemListQuery
{
    public string? Search { get; init; }
    public ProblemStatus? Status { get; init; }
    public int? Page { get; init; }
    public int? PageSize { get; init; }
}

/// <summary>Resolved guest context for the best-effort anonymous review limit.</summary>
public record GuestContext(string? RawGuestId, string? GuestIdHash, string? IpHash);

/// <summary>Identity of a conversation created from an accepted inquiry.</summary>
public record ConversationRef(int ConversationId);

public record IdeaScreeningDecisionDto
{
    public int Id { get; init; }
    public IdeaStatus Decision { get; init; }
    public int? OriginalityScore { get; init; }
    public int? FeasibilityScore { get; init; }
    public int? RelevanceScore { get; init; }
    public int? ImpactScore { get; init; }
    public string? Feedback { get; init; }
    public DateTime CreatedAt { get; init; }
}
