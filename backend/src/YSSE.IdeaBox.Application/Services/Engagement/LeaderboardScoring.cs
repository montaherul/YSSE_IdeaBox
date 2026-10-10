namespace YSSE.IdeaBox.Application.Services.Engagement;

/// <summary>
/// Idea-maker leaderboard scoring (FR-E12). Each metric is normalized against the
/// highest value present and weighted 40/25/15/15/5; SafeMax prevents divide-by-zero
/// when a metric is absent across all rows. Extracted so the weighting rule is pure
/// and unit-testable.
/// </summary>
internal static class LeaderboardScoring
{
    public static double SafeMax(IEnumerable<int> values)
    {
        var max = values.DefaultIfEmpty().Max();
        return max > 0 ? max : 1;
    }

    public static double Score(int votes, int published, int shortlisted, int outcomes, int contributions,
        double maxVotes, double maxPublished, double maxShortlisted, double maxOutcomes, double maxContributions) =>
        Math.Round(
            40.0 * votes / Norm(maxVotes) +
            25.0 * published / Norm(maxPublished) +
            15.0 * shortlisted / Norm(maxShortlisted) +
            15.0 * outcomes / Norm(maxOutcomes) +
            5.0 * contributions / Norm(maxContributions), 2);

    private static double Norm(double max) => max > 0 ? max : 1;
}