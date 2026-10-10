using YSSE.IdeaBox.Application.Services.Engagement;
using YSSE.IdeaBox.Domain.Enums;

namespace YSSE.IdeaBox.Tests;

/// <summary>Opportunity-match ranking (FR-H12): 0.4 industry, 0.25 × interest hits (max 2), 0.2 expertise, 0.15 stage.</summary>
public class MatchingScorerTests
{
    [Fact]
    public void No_signals_returns_zero_score_and_no_reasons()
    {
        var (score, reasons) = MatchingScorer.ScoreIdea(
            "Fintech", "AI", null, DevelopmentStage.Concept,
            "Healthcare", "Travel", "Baking");

        Assert.Equal(0, score);
        Assert.Empty(reasons);
    }

    [Fact]
    public void Industry_match_adds_0_4()
    {
        var (score, reasons) = MatchingScorer.ScoreIdea(
            "Fintech", "AI", null, DevelopmentStage.Concept,
            "Fintech, Data", null, null);

        Assert.Equal(0.4, score);
        Assert.Contains(reasons, r => r.StartsWith("Matches your industry"));
    }

    [Fact]
    public void Expertise_alignment_adds_0_2()
    {
        var (score, reasons) = MatchingScorer.ScoreIdea(
            "Fintech", "AI", "Python, Django", DevelopmentStage.Concept,
            null, null, "Python, Postgres");

        Assert.Equal(0.2, score);
        Assert.Contains(reasons, r => r == "Aligns with your expertise");
    }

    [Theory]
    [InlineData(DevelopmentStage.Pilot)]
    [InlineData(DevelopmentStage.EarlyRevenue)]
    [InlineData(DevelopmentStage.Scaling)]
    public void Active_collaboration_stage_adds_0_15(DevelopmentStage stage)
    {
        var (score, reasons) = MatchingScorer.ScoreIdea(
            "Fintech", "AI", null, stage, null, null, null);

        Assert.Equal(0.15, score);
        Assert.Contains(reasons, r => r.Contains("Active project"));
    }

    [Theory]
    [InlineData(DevelopmentStage.Concept)]
    [InlineData(DevelopmentStage.Prototype)]
    public void Early_stages_do_not_add_collaboration_bonus(DevelopmentStage stage)
    {
        var (score, _) = MatchingScorer.ScoreIdea(
            "Fintech", "AI", null, stage, null, null, null);

        Assert.Equal(0, score);
    }

    [Fact]
    public void Interest_hits_are_capped_at_two()
    {
        var (score, reasons) = MatchingScorer.ScoreIdea(
            "Fintech", "AI, Blockchain, Crypto", null, DevelopmentStage.Concept,
            null, "AI, Blockchain, Crypto, Defi", null);

        Assert.Equal(0.5, score);   // 2 x 0.25, third hit ignored
        Assert.Contains(reasons, r => r.StartsWith("Related to your interests"));
    }

    [Fact]
    public void Composite_match_sums_all_weighted_signals()
    {
        var (score, reasons) = MatchingScorer.ScoreIdea(
            "Fintech", "Blockchain, AI", "Python, Django", DevelopmentStage.Pilot,
            "Fintech, Data", "Blockchain, AI", "Python");

        Assert.Equal(0.4 + 0.5 + 0.2 + 0.15, score);   // 1.25
        Assert.Equal(4, reasons.Count);
    }

    [Fact]
    public void Reason_references_the_seekers_industry_verbatim()
    {
var (_, reasons) = MatchingScorer.ScoreIdea(
            "Fintech", null, null, DevelopmentStage.Concept,
            "Fintech, Financial Services", null, null);

    Assert.Contains(reasons, r => r == "Matches your industry (Fintech, Financial Services)");
    }

    [Fact]
    public void Tokenize_splits_on_common_delimiters_and_drops_single_chars()
    {
        var tokens = MatchingScorer.Tokenize(" AI,Blockchain;Data|Pilot\nEnglish ");
        Assert.Equal(new[] { "AI", "Blockchain", "Data", "Pilot", "English" }, tokens);
    }
}