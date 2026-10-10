using YSSE.IdeaBox.Domain.Enums;

namespace YSSE.IdeaBox.Application.Services.Engagement;

/// <summary>
/// Opportunity-match ranking (FR-H12). Scoring is weighted: 0.4 industry match,
/// 0.25 per interest-tag hit (capped at two), 0.2 expertise alignment, 0.15 active
/// collaboration stage. Extracted so the rule is pure and unit-testable; the service
/// supplies pre-joined idea/tag data exactly as the original rule consumed it.
/// </summary>
internal static class MatchingScorer
{
    public static IEnumerable<string> Tokenize(string? input) =>
        (input ?? string.Empty)
            .Split(',', ';', '|', '\n')
            .Select(t => t.Trim())
            .Where(t => t.Length > 1)
            .Distinct(StringComparer.OrdinalIgnoreCase);

    public static (double Score, List<string> Reasons) ScoreIdea(
        string? categoryName,
        string? tagNames,
        string? skillsNeeded,
        DevelopmentStage stage,
        string? industry,
        string? interestAreas,
        string? expertise)
    {
        var score = 0.0;
        var reasons = new List<string>();

        var industryTokens = Tokenize(industry);
        var interestTokens = Tokenize(interestAreas);
        var expertiseTokens = Tokenize(expertise);

        var category = categoryName ?? string.Empty;
        if (industryTokens.Any(t => t.Contains(category, StringComparison.OrdinalIgnoreCase) ||
                                    category.Contains(t, StringComparison.OrdinalIgnoreCase)))
        {
            score += 0.4;
            reasons.Add($"Matches your industry ({industry})");
        }

        var tagList = tagNames ?? string.Empty;
        var interestHits = interestTokens.Where(t => !string.IsNullOrWhiteSpace(t) &&
            tagList.Contains(t, StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(2)
            .ToList();
        if (interestHits.Count > 0)
        {
            score += 0.25 * interestHits.Count;
            reasons.Add($"Related to your interests ({string.Join(", ", interestHits)})");
        }

        if (!string.IsNullOrWhiteSpace(skillsNeeded) &&
            expertiseTokens.Any(t => skillsNeeded.Contains(t, StringComparison.OrdinalIgnoreCase)))
        {
            score += 0.2;
            reasons.Add("Aligns with your expertise");
        }

        if (stage is DevelopmentStage.Pilot or DevelopmentStage.EarlyRevenue or DevelopmentStage.Scaling)
        {
            score += 0.15;
            reasons.Add("Active project looking for collaboration");
        }

        return (Math.Round(score, 2), reasons);
    }
}