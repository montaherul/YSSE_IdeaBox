using System.ComponentModel.DataAnnotations;
using YSSE.IdeaBox.Domain.Enums;

namespace YSSE.IdeaBox.Application.Dtos;

public record VoteResultDto
{
    public int IdeaId { get; init; }
    public bool Voted { get; init; }
    public int VoteCount { get; init; }
    public bool CountIsVisible { get; init; }
}

public record VotingPeriodDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public DateTime StartAt { get; init; }
    public DateTime EndAt { get; init; }
    public VoteVisibility Visibility { get; init; }
    public bool IsActive { get; init; }
    public bool IsClosed { get; init; }
}

public record VotingPeriodCreateRequest
{
    [Required, MaxLength(150)] public string Name { get; init; } = string.Empty;
    public DateTime StartAt { get; init; }
    public DateTime EndAt { get; init; }
    public VoteVisibility Visibility { get; init; } = VoteVisibility.HiddenUntilClose;
    [Range(0, 3650)] public int MinimumAccountAgeDays { get; init; } = 7;
    public bool RequiresVerifiedAccount { get; init; } = true;
    public bool IsActive { get; init; } = true;
}

public record LeaderboardIdeaDto
{
    public int Rank { get; init; }
    public int IdeaId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string CategoryName { get; init; } = string.Empty;
    public IdeaStatus Status { get; init; }
    public long VoteCount { get; init; }
}

public record IdeaMakerLeaderboardDto
{
    public int Rank { get; init; }
    public int UserId { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public int VotesReceived { get; init; }
    public int PublishedIdeas { get; init; }
    public int ShortlistedIdeas { get; init; }
    public int VerifiedOutcomes { get; init; }
    public double Score { get; init; }
}
