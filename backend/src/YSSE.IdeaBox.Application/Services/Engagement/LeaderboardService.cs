using YSSE.IdeaBox.Application.Abstractions;
using YSSE.IdeaBox.Application.Abstractions.Services;
using YSSE.IdeaBox.Application.Dtos;
using YSSE.IdeaBox.Domain.Entities;
using YSSE.IdeaBox.Domain.Enums;

namespace YSSE.IdeaBox.Application.Services.Engagement;

public sealed class LeaderboardService : ILeaderboardService
{
    private readonly IUnitOfWork _unitOfWork;

    public LeaderboardService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<LeaderboardIdeaDto>> GetTopIdeasAsync(int? votingPeriodId, int take = 3, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 50);

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

        var rows = await _unitOfWork.Repository<Idea>().ExecuteQueryAsync<IdeaRow>(sql, new { VotingPeriodId = votingPeriodId, Take = take }, ct);

        var result = new List<LeaderboardIdeaDto>(rows.Count);
        var rank = 1;
        foreach (var row in rows)
        {
            result.Add(new LeaderboardIdeaDto
            {
                Rank = rank++,
                IdeaId = row.IdeaId,
                Title = row.Title,
                Slug = row.Slug,
                CategoryName = row.CategoryName,
                Status = (IdeaStatus)row.Status,
                VoteCount = row.VoteCount
            });
        }

        return result;
    }

    public async Task<IReadOnlyList<IdeaMakerLeaderboardDto>> GetTopIdeaMakersAsync(int? votingPeriodId, int take = 3, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 50);

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

        var rows = await _unitOfWork.Repository<Idea>().ExecuteQueryAsync<MakerRow>(sql, new { VotingPeriodId = votingPeriodId, Take = take }, ct);

var maxVotes = LeaderboardScoring.SafeMax(rows.Select(r => r.VotesReceived));
        var maxPublished = LeaderboardScoring.SafeMax(rows.Select(r => r.PublishedIdeas));
        var maxShortlisted = LeaderboardScoring.SafeMax(rows.Select(r => r.ShortlistedIdeas));
        var maxOutcomes = LeaderboardScoring.SafeMax(rows.Select(r => r.VerifiedOutcomes));
        var maxContributions = LeaderboardScoring.SafeMax(rows.Select(r => r.VerifiedContributions));

        var scored = rows.Select(row => new ScoredMaker
        {
            Row = row,
            Score = LeaderboardScoring.Score(
                row.VotesReceived, row.PublishedIdeas, row.ShortlistedIdeas,
                row.VerifiedOutcomes, row.VerifiedContributions,
                maxVotes, maxPublished, maxShortlisted, maxOutcomes, maxContributions)
        })
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.Row.UserId)
            .ToList();

        var result = new List<IdeaMakerLeaderboardDto>(scored.Count);
        var rank = 1;
        foreach (var item in scored)
        {
            result.Add(new IdeaMakerLeaderboardDto
            {
                Rank = rank++,
                UserId = item.Row.UserId,
                DisplayName = item.Row.DisplayName,
                VotesReceived = item.Row.VotesReceived,
                PublishedIdeas = item.Row.PublishedIdeas,
                ShortlistedIdeas = item.Row.ShortlistedIdeas,
                VerifiedOutcomes = item.Row.VerifiedOutcomes,
                Score = item.Score
            });
        }

        return result;
    }

private sealed class IdeaRow
    {
        public int IdeaId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int Status { get; set; }
        public long VoteCount { get; set; }
    }

    private sealed class MakerRow
    {
        public int UserId { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public int VotesReceived { get; set; }
        public int PublishedIdeas { get; set; }
        public int ShortlistedIdeas { get; set; }
        public int VerifiedOutcomes { get; set; }
        public int VerifiedContributions { get; set; }
    }

    private sealed class ScoredMaker
    {
        public MakerRow Row { get; set; } = new();
        public double Score { get; set; }
    }
}

