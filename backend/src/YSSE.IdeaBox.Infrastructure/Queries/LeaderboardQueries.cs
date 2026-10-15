using YSSE.IdeaBox.Application.Abstractions;

namespace YSSE.IdeaBox.Infrastructure.Queries;

/// <summary>
/// Dapper-backed read model for the leaderboards. The aggregated SQL is PostgreSQL-specific
/// and kept in the Infrastructure layer; the optional voting-period filter is parameterized.
/// </summary>
public sealed class LeaderboardQueries : ILeaderboardQueries
{
    private readonly ISqlQueryExecutor _executor;

    public LeaderboardQueries(ISqlQueryExecutor executor) => _executor = executor;

    public async Task<IReadOnlyList<LeaderboardIdeaRow>> GetTopIdeasAsync(
        int? votingPeriodId, int take, CancellationToken ct = default)
    {
        var periodFilter = votingPeriodId is null ? string.Empty : "AND v.voting_period_id = @VotingPeriodId";
        var sql = $@"
SELECT i.id AS ""IdeaId"",
       i.title AS ""Title"",
       i.slug AS ""Slug"",
       COALESCE(c.name, '') AS ""CategoryName"",
       i.status AS ""Status"",
       COUNT(v.id) AS ""VoteCount""
FROM ideas i
LEFT JOIN categories c ON c.id = i.category_id
LEFT JOIN idea_votes v ON v.idea_id = i.id AND v.is_deleted = false AND v.is_counted = true {periodFilter}
WHERE i.is_deleted = false AND i.status IN (4, 6)
GROUP BY i.id, i.title, i.slug, c.name, i.status
ORDER BY COUNT(v.id) DESC, i.id ASC
LIMIT @Take";

        return await _executor.QueryAsync<LeaderboardIdeaRow>(
            sql, new { VotingPeriodId = votingPeriodId, Take = take }, ct);
    }

    public async Task<IReadOnlyList<LeaderboardMakerRow>> GetTopIdeaMakersAsync(
        int? votingPeriodId, int take, CancellationToken ct = default)
    {
        var periodFilter = votingPeriodId is null ? string.Empty : "AND v.voting_period_id = @VotingPeriodId";
        var sql = $@"
SELECT t.creator_user_id AS ""UserId"",
       u.display_name AS ""DisplayName"",
       CAST(COALESCE(v.votes_received, 0) AS integer) AS ""VotesReceived"",
       CAST(COALESCE(p.published_ideas, 0) AS integer) AS ""PublishedIdeas"",
       CAST(COALESCE(s.shortlisted_ideas, 0) AS integer) AS ""ShortlistedIdeas"",
       CAST(COALESCE(o.verified_outcomes, 0) AS integer) AS ""VerifiedOutcomes"",
       CAST(COALESCE(pm.contributions, 0) AS integer) AS ""VerifiedContributions""
FROM (
    SELECT DISTINCT creator_user_id FROM ideas WHERE is_deleted = false AND status IN (4, 6, 7, 8, 9)
) t
JOIN users u ON u.id = t.creator_user_id
LEFT JOIN (
    SELECT i.creator_user_id, COUNT(*) AS votes_received
    FROM idea_votes v
    JOIN ideas i ON i.id = v.idea_id
    WHERE v.is_counted = true AND v.is_deleted = false AND i.is_deleted = false {periodFilter}
    GROUP BY i.creator_user_id
) v ON v.creator_user_id = t.creator_user_id
LEFT JOIN (
    SELECT creator_user_id, COUNT(*) AS published_ideas
    FROM ideas WHERE is_deleted = false AND status = 4
    GROUP BY creator_user_id
) p ON p.creator_user_id = t.creator_user_id
LEFT JOIN (
    SELECT creator_user_id, COUNT(*) AS shortlisted_ideas
    FROM ideas WHERE is_deleted = false AND status IN (6, 7, 8, 9)
    GROUP BY creator_user_id
) s ON s.creator_user_id = t.creator_user_id
LEFT JOIN (
    SELECT i.creator_user_id, COUNT(*) AS verified_outcomes
    FROM opportunity_outcomes o
    JOIN ideas i ON i.id = o.idea_id
    WHERE o.is_verified = true
    GROUP BY i.creator_user_id
) o ON o.creator_user_id = t.creator_user_id
LEFT JOIN (
    SELECT user_id, COUNT(DISTINCT project_id) AS contributions
    FROM project_members
    GROUP BY user_id
) pm ON pm.user_id = t.creator_user_id
ORDER BY v.votes_received DESC NULLS LAST, t.creator_user_id ASC
LIMIT @Take";

        return await _executor.QueryAsync<LeaderboardMakerRow>(
            sql, new { VotingPeriodId = votingPeriodId, Take = take }, ct);
    }
}
