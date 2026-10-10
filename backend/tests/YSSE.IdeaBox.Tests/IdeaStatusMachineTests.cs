using YSSE.IdeaBox.Application.Services.Common;
using YSSE.IdeaBox.Domain.Enums;

namespace YSSE.IdeaBox.Tests;

/// <summary>Idea lifecycle transition table (SRS §7.3, FR-C09). Any transition not listed is rejected.</summary>
public class IdeaStatusMachineTests
{
    [Theory]
    [InlineData(IdeaStatus.Draft, IdeaStatus.Submitted)]
    [InlineData(IdeaStatus.Draft, IdeaStatus.Withdrawn)]
    [InlineData(IdeaStatus.Submitted, IdeaStatus.UnderReview)]
    [InlineData(IdeaStatus.Submitted, IdeaStatus.Withdrawn)]
    [InlineData(IdeaStatus.UnderReview, IdeaStatus.Published)]
    [InlineData(IdeaStatus.UnderReview, IdeaStatus.Rejected)]
    [InlineData(IdeaStatus.UnderReview, IdeaStatus.RevisionRequired)]
    [InlineData(IdeaStatus.RevisionRequired, IdeaStatus.Submitted)]
    [InlineData(IdeaStatus.RevisionRequired, IdeaStatus.Withdrawn)]
    [InlineData(IdeaStatus.Published, IdeaStatus.Shortlisted)]
    [InlineData(IdeaStatus.Published, IdeaStatus.Archived)]
    [InlineData(IdeaStatus.Published, IdeaStatus.Withdrawn)]
    [InlineData(IdeaStatus.Published, IdeaStatus.UnderReview)]
    [InlineData(IdeaStatus.Shortlisted, IdeaStatus.ApprovedForDevelopment)]
    [InlineData(IdeaStatus.Shortlisted, IdeaStatus.Completed)]
    [InlineData(IdeaStatus.Shortlisted, IdeaStatus.Archived)]
    [InlineData(IdeaStatus.ApprovedForDevelopment, IdeaStatus.InProgress)]
    [InlineData(IdeaStatus.ApprovedForDevelopment, IdeaStatus.Archived)]
    [InlineData(IdeaStatus.InProgress, IdeaStatus.Completed)]
    [InlineData(IdeaStatus.InProgress, IdeaStatus.Archived)]
    [InlineData(IdeaStatus.Completed, IdeaStatus.Archived)]
    [InlineData(IdeaStatus.Rejected, IdeaStatus.Archived)]
    [InlineData(IdeaStatus.Withdrawn, IdeaStatus.Archived)]
    public void CanTransition_allows_documented_transitions(IdeaStatus from, IdeaStatus to)
        => Assert.True(IdeaStatusMachine.CanTransition(from, to));

    [Theory]
    [InlineData(IdeaStatus.Draft, IdeaStatus.Published)]
    [InlineData(IdeaStatus.Draft, IdeaStatus.Shortlisted)]
    [InlineData(IdeaStatus.Draft, IdeaStatus.UnderReview)]
    [InlineData(IdeaStatus.Draft, IdeaStatus.Rejected)]
    [InlineData(IdeaStatus.Submitted, IdeaStatus.Published)]
    [InlineData(IdeaStatus.Submitted, IdeaStatus.Completed)]
    [InlineData(IdeaStatus.Submitted, IdeaStatus.ApprovedForDevelopment)]
    [InlineData(IdeaStatus.UnderReview, IdeaStatus.Shortlisted)]
    [InlineData(IdeaStatus.UnderReview, IdeaStatus.Withdrawn)]
    [InlineData(IdeaStatus.RevisionRequired, IdeaStatus.UnderReview)]
    [InlineData(IdeaStatus.RevisionRequired, IdeaStatus.Published)]
    [InlineData(IdeaStatus.RevisionRequired, IdeaStatus.Shortlisted)]
    [InlineData(IdeaStatus.Published, IdeaStatus.Submitted)]
    [InlineData(IdeaStatus.Published, IdeaStatus.Rejected)]
    [InlineData(IdeaStatus.Published, IdeaStatus.ApprovedForDevelopment)]
    [InlineData(IdeaStatus.Shortlisted, IdeaStatus.Published)]
    [InlineData(IdeaStatus.Shortlisted, IdeaStatus.Withdrawn)]
    [InlineData(IdeaStatus.Shortlisted, IdeaStatus.InProgress)]
    [InlineData(IdeaStatus.ApprovedForDevelopment, IdeaStatus.Published)]
    [InlineData(IdeaStatus.ApprovedForDevelopment, IdeaStatus.Completed)]
    [InlineData(IdeaStatus.InProgress, IdeaStatus.Shortlisted)]
    [InlineData(IdeaStatus.InProgress, IdeaStatus.Published)]
    [InlineData(IdeaStatus.Completed, IdeaStatus.Published)]
    [InlineData(IdeaStatus.Completed, IdeaStatus.InProgress)]
    [InlineData(IdeaStatus.Rejected, IdeaStatus.Published)]
    [InlineData(IdeaStatus.Rejected, IdeaStatus.Shortlisted)]
    [InlineData(IdeaStatus.Rejected, IdeaStatus.RevisionRequired)]
    [InlineData(IdeaStatus.Withdrawn, IdeaStatus.Published)]
    [InlineData(IdeaStatus.Withdrawn, IdeaStatus.Submitted)]
    [InlineData(IdeaStatus.Archived, IdeaStatus.Published)]
    [InlineData(IdeaStatus.Archived, IdeaStatus.UnderReview)]
    [InlineData(IdeaStatus.Archived, IdeaStatus.Draft)]
    public void CanTransition_rejects_out_of_band_transitions(IdeaStatus from, IdeaStatus to)
        => Assert.False(IdeaStatusMachine.CanTransition(from, to));

    [Fact]
    public void No_status_transitions_to_itself()
        => Assert.All(Enum.GetValues<IdeaStatus>(), s => Assert.False(IdeaStatusMachine.CanTransition(s, s)));
}