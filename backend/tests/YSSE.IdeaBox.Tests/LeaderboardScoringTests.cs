using YSSE.IdeaBox.Application.Services.Engagement;

namespace YSSE.IdeaBox.Tests;

/// <summary>Idea-maker leaderboard weighting (FR-E12): 40/25/15/15/5 normalized to row maxima.</summary>
public class LeaderboardScoringTests
{
    [Fact]
    public void SafeMax_returns_one_for_empty_or_all_zero_sources()
    {
        Assert.Equal(1, LeaderboardScoring.SafeMax(Array.Empty<int>()));
        Assert.Equal(1, LeaderboardScoring.SafeMax(new[] { 0, 0, 0 }));
        Assert.Equal(1, LeaderboardScoring.SafeMax(new[] { -2, -1 }));
    }

    [Fact]
    public void SafeMax_returns_the_largest_value_when_present()
    {
        Assert.Equal(9, LeaderboardScoring.SafeMax(new[] { 3, 9, 1, 0 }));
    }

    [Fact]
    public void Score_combines_weighted_normalized_metrics()
    {
        var score = LeaderboardScoring.Score(
            votes: 10, published: 5, shortlisted: 2, outcomes: 0, contributions: 4,
            maxVotes: 20, maxPublished: 5, maxShortlisted: 4, maxOutcomes: 3, maxContributions: 8);

        Assert.Equal(20 + 25 + 7.5 + 0 + 2.5, score);   // 55
    }

    [Fact]
    public void Score_reaches_full_marks_when_all_metrics_are_at_maximums()
    {
        var score = LeaderboardScoring.Score(
            votes: 12, published: 4, shortlisted: 3, outcomes: 2, contributions: 6,
            maxVotes: 12, maxPublished: 4, maxShortlisted: 3, maxOutcomes: 2, maxContributions: 6);

        Assert.Equal(100, score);   // 40 + 25 + 15 + 15 + 5
    }

    [Fact]
    public void Score_never_divides_by_zero_when_maxes_are_absent()
    {
        var score = LeaderboardScoring.Score(
            votes: 3, published: 1, shortlisted: 0, outcomes: 0, contributions: 0,
            maxVotes: 0, maxPublished: 0, maxShortlisted: 0, maxOutcomes: 0, maxContributions: 0);

        // All SafeMax → 1, so: (40*3 + 25*1 + 0 + 0 + 0) = 145
        Assert.Equal(145, score);
    }

    [Fact]
    public void Score_rounds_to_two_decimal_places()
    {
        var score = LeaderboardScoring.Score(
            votes: 1, published: 1, shortlisted: 1, outcomes: 1, contributions: 1,
            maxVotes: 3, maxPublished: 3, maxShortlisted: 3, maxOutcomes: 3, maxContributions: 3);

        // (40 + 25 + 15 + 15 + 5) / 3 = 100/3 ≈ 33.333 -> 33.33
        Assert.Equal(33.33, score);
    }
}