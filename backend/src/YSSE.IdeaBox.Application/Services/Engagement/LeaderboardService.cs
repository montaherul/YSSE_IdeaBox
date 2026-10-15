using Microsoft.EntityFrameworkCore;
using YSSE.IdeaBox.Application.Abstractions;
using YSSE.IdeaBox.Application.Abstractions.Services;
using YSSE.IdeaBox.Application.Dtos;
using YSSE.IdeaBox.Domain.Entities;
using YSSE.IdeaBox.Domain.Enums;

namespace YSSE.IdeaBox.Application.Services.Engagement;

public sealed class LeaderboardService : ILeaderboardService
{
    private const int MonthlyTopSize = 3;

    private readonly ILeaderboardQueries _queries;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationDispatcher _notifications;
    private readonly IDateTimeProvider _clock;

    public LeaderboardService(
        ILeaderboardQueries queries,
        IUnitOfWork unitOfWork,
        INotificationDispatcher notifications,
        IDateTimeProvider clock)
    {
        _queries = queries;
        _unitOfWork = unitOfWork;
        _notifications = notifications;
        _clock = clock;
    }

    public async Task<IReadOnlyList<LeaderboardIdeaDto>> GetTopIdeasAsync(int? votingPeriodId, int take = 3, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 50);

        var rows = await _queries.GetTopIdeasAsync(votingPeriodId, take, ct);

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

        var rows = await _queries.GetTopIdeaMakersAsync(votingPeriodId, take, ct);

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

    public async Task<MonthlyTopDto> GetMonthlyTopIdeasAsync(int take = 3, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 50);
        var (period, label) = await ResolveCurrentPeriodAsync(ct);
        var ideas = await GetTopIdeasAsync(period?.Id, take, ct);

        return new MonthlyTopDto
        {
            VotingPeriodId = period?.Id,
            PeriodLabel = label,
            Ideas = ideas
        };
    }

    public async Task<RecognitionResultDto> AnnounceMonthlyTopAsync(string? message = null, CancellationToken ct = default)
    {
        var (period, label) = await ResolveCurrentPeriodAsync(ct);
        var top = await GetTopIdeasAsync(period?.Id, MonthlyTopSize, ct);
        if (top.Count == 0)
        {
            return new RecognitionResultDto { PeriodLabel = label, NotifiedCount = 0 };
        }

        var ideaIds = top.Select(t => t.IdeaId).ToList();
        var rows = await _unitOfWork.Repository<Idea>().Query()
            .Where(i => ideaIds.Contains(i.Id))
            .Select(i => new { i.Id, i.CreatorUserId, i.Title, i.Slug })
            .ToListAsync(ct);
        var byId = rows.ToDictionary(r => r.Id);

        var notified = 0;
        foreach (var item in top)
        {
            if (!byId.TryGetValue(item.IdeaId, out var row))
            {
                continue;
            }

            var body = string.IsNullOrWhiteSpace(message)
                ? $"Congratulations! Your idea \"{row.Title}\" is ranked #{item.Rank} among the top {top.Count} ideas for {label}. Thank you for contributing to the community."
                : message!;

            await _notifications.NotifyAsync(
                row.CreatorUserId,
                NotificationType.LeaderboardResult,
                $"Top {top.Count} idea — {label}",
                body,
                $"/ideas/{row.Slug}",
                essential: true,
                ct);
            notified++;
        }

        return new RecognitionResultDto { PeriodLabel = label, NotifiedCount = notified };
    }

    private async Task<(VotingPeriod? Period, string Label)> ResolveCurrentPeriodAsync(CancellationToken ct)
    {
        var now = _clock.UtcNow;
        var periods = _unitOfWork.Repository<VotingPeriod>().Query();

        var current = await periods
            .Where(p => p.IsActive && !p.IsClosed && p.StartAt <= now && p.EndAt >= now)
            .OrderByDescending(p => p.StartAt)
            .FirstOrDefaultAsync(ct);

        if (current is not null)
        {
            return (current, LabelFor(current));
        }

        var latest = await periods.OrderByDescending(p => p.StartAt).FirstOrDefaultAsync(ct);
        return latest is not null ? (latest, LabelFor(latest)) : (null, now.ToString("yyyy-MM"));
    }

    private static string LabelFor(VotingPeriod period) =>
        string.IsNullOrWhiteSpace(period.Name) ? period.StartAt.ToString("yyyy-MM") : period.Name;

    private sealed class ScoredMaker
    {
        public LeaderboardMakerRow Row { get; set; } = new();
        public double Score { get; set; }
    }
}
