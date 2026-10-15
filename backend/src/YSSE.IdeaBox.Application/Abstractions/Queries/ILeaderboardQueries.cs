namespace YSSE.IdeaBox.Application.Abstractions;

/// <summary>
/// Read-model queries for the leaderboards. Returns raw aggregated rows; ranking and
/// scoring rules stay in the Application layer. The SQL lives in the Infrastructure
/// implementation so the Application layer stays free of persistence concerns.
/// </summary>
public interface ILeaderboardQueries
{
    Task<IReadOnlyList<LeaderboardIdeaRow>> GetTopIdeasAsync(
        int? votingPeriodId, int take, CancellationToken ct = default);

    Task<IReadOnlyList<LeaderboardMakerRow>> GetTopIdeaMakersAsync(
        int? votingPeriodId, int take, CancellationToken ct = default);
}

/// <summary>Raw row for the top-ideas leaderboard (mapped from SQL column aliases).</summary>
public sealed class LeaderboardIdeaRow
{
    public int IdeaId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int Status { get; set; }
    public long VoteCount { get; set; }
}

/// <summary>Raw row for the idea-maker leaderboard (mapped from SQL column aliases).</summary>
public sealed class LeaderboardMakerRow
{
    public int UserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public int VotesReceived { get; set; }
    public int PublishedIdeas { get; set; }
    public int ShortlistedIdeas { get; set; }
    public int VerifiedOutcomes { get; set; }
    public int VerifiedContributions { get; set; }
}
