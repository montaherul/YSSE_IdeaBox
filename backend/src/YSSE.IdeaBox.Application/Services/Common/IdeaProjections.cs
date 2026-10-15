using System.Linq.Expressions;
using YSSE.IdeaBox.Application.Dtos;
using YSSE.IdeaBox.Domain.Entities;
using YSSE.IdeaBox.Domain.Enums;

namespace YSSE.IdeaBox.Application.Services.Common;

/// <summary>Shared EF projections so list/detail mapping stays in one place.</summary>
internal static class IdeaProjections
{
    public static readonly Expression<Func<Idea, IdeaListItemDto>> List = x => new IdeaListItemDto
    {
        Id = x.Id,
        Slug = x.Slug,
        Title = x.Title,
        ShortSummary = x.ShortSummary,
        CategoryName = x.Category != null ? x.Category.Name : string.Empty,
        Stage = x.Stage,
        Status = x.Status,
        IsFeatured = x.IsFeatured,
        IsAnonymous = x.Attribution == IdeaAttribution.Anonymous,
        CreatorAlias = x.Attribution == IdeaAttribution.Anonymous
            ? null
            : (x.Creator.CreatorProfile != null && !x.Creator.CreatorProfile.IsDeleted
                ? x.Creator.CreatorProfile.Alias
                : x.Creator.DisplayName),
        RegisteredReviewCount = x.Reviews.Count(r => !r.IsDeleted && !r.IsGuest && r.ModerationStatus == ReviewModerationStatus.Published),
        GuestReviewCount = x.Reviews.Count(r => !r.IsDeleted && r.IsGuest && r.ModerationStatus == ReviewModerationStatus.Published),
        VoteCount = x.Votes.Count(v => !v.IsDeleted && v.IsCounted),
        PublishedAt = x.PublishedAt,
        LanguageTag = x.LanguageTag,
        Tags = x.IdeaTags.Select(t => t.Tag.Name).ToList()
    };

    /// <summary>
    /// Detail projection. Attachments are private: they are only materialized for the
    /// idea's owner (<paramref name="currentUserId"/>) or when the caller holds the
    /// attachment read permission (<paramref name="includeAllAttachments"/>). Pass 0
    /// when there is no authenticated caller.
    /// </summary>
    public static Expression<Func<Idea, IdeaDetailDto>> Detail(int currentUserId, bool includeAllAttachments) => x => new IdeaDetailDto
    {
        Id = x.Id,
        Slug = x.Slug,
        Title = x.Title,
        ShortSummary = x.ShortSummary,
        CategoryName = x.Category != null ? x.Category.Name : string.Empty,
        Stage = x.Stage,
        Status = x.Status,
        IsFeatured = x.IsFeatured,
        IsAnonymous = x.Attribution == IdeaAttribution.Anonymous,
        CreatorAlias = x.Attribution == IdeaAttribution.Anonymous
            ? null
            : (x.Creator.CreatorProfile != null && !x.Creator.CreatorProfile.IsDeleted
                ? x.Creator.CreatorProfile.Alias
                : x.Creator.DisplayName),
        RegisteredReviewCount = x.Reviews.Count(r => !r.IsDeleted && !r.IsGuest && r.ModerationStatus == ReviewModerationStatus.Published),
        GuestReviewCount = x.Reviews.Count(r => !r.IsDeleted && r.IsGuest && r.ModerationStatus == ReviewModerationStatus.Published),
        VoteCount = x.Votes.Count(v => !v.IsDeleted && v.IsCounted),
        PublishedAt = x.PublishedAt,
        LanguageTag = x.LanguageTag,
        Tags = x.IdeaTags.Select(t => t.Tag.Name).ToList(),
        ProblemStatement = x.ProblemStatement,
        ProposedSolution = x.ProposedSolution,
        TargetAudience = x.TargetAudience,
        ExpectedImpact = x.ExpectedImpact,
        SupportSought = x.SupportSought,
        SkillsNeeded = x.SkillsNeeded,
        EstimatedBudget = x.EstimatedBudget,
        ExpectedTimeline = x.ExpectedTimeline,
        SuccessMetrics = x.SuccessMetrics,
        CollaborationPreferences = x.CollaborationPreferences,
        ViewsCount = x.ViewsCount,
        CanEdit = currentUserId != 0 && x.CreatorUserId == currentUserId &&
                  (x.Status == IdeaStatus.Draft || x.Status == IdeaStatus.RevisionRequired),
        Attachments = x.Attachments
            .Where(a => !a.IsDeleted && (includeAllAttachments || x.CreatorUserId == currentUserId))
            .Select(a => new IdeaAttachmentDto
            {
                Id = a.Id,
                FileName = a.FileName,
                ContentType = a.ContentType,
                SizeBytes = a.SizeBytes
            }).ToList(),
        StatusHistory = x.StatusHistory.Select(h => new IdeaStatusHistoryDto
        {
            FromStatus = h.FromStatus,
            ToStatus = h.ToStatus,
            Reason = h.Reason,
            CreatedAt = h.CreatedAt
        }).ToList()
    };
}

