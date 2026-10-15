using System.Text;
using YSSE.IdeaBox.Application.Abstractions;
using YSSE.IdeaBox.Application.Common;
using YSSE.IdeaBox.Domain.Enums;

namespace YSSE.IdeaBox.Application.Services.Common;

/// <summary>Slug generation shared by modules (ideas, categories, tags).</summary>
internal static class Slug
{
    public static string From(string? input, string fallback = "item")
    {
        if (!string.IsNullOrWhiteSpace(input))
        {
            var lower = input.Trim().ToLowerInvariant();
            var sb = new StringBuilder(lower.Length);
            var lastDash = false;
            foreach (var ch in lower)
            {
                if (char.IsLetterOrDigit(ch))
                {
                    sb.Append(ch);
                    lastDash = false;
                }
                else if (!lastDash)
                {
                    sb.Append('-');
                    lastDash = true;
                }
            }

            var result = sb.ToString().Trim('-');
            if (result.Length > 0)
            {
                return result.Length > 120 ? result[..120] : result;
            }
        }

        return $"{fallback}-{Guid.NewGuid():N}"[..Math.Min(fallback.Length + 9, 32)];
    }
}

internal static class CurrentUserGuard
{
    public static int RequireUserId(this ICurrentUserService user) =>
        user.UserId ?? throw new UnauthorizedException();
}


/// <summary>
/// Idea lifecycle transition table (SRS §7.3). Any transition not listed is rejected.
/// </summary>
internal static class IdeaStatusMachine
{
    private static readonly Dictionary<IdeaStatus, HashSet<IdeaStatus>> Allowed = new()
    {
        [IdeaStatus.Draft] = new() { IdeaStatus.Submitted, IdeaStatus.Withdrawn },
        [IdeaStatus.Submitted] = new() { IdeaStatus.UnderReview, IdeaStatus.Withdrawn },
        [IdeaStatus.UnderReview] = new() { IdeaStatus.Published, IdeaStatus.Rejected, IdeaStatus.RevisionRequired },
        [IdeaStatus.RevisionRequired] = new() { IdeaStatus.Submitted, IdeaStatus.Withdrawn },
        [IdeaStatus.Published] = new() { IdeaStatus.Shortlisted, IdeaStatus.Archived, IdeaStatus.Withdrawn, IdeaStatus.UnderReview },
        [IdeaStatus.Shortlisted] = new() { IdeaStatus.ApprovedForDevelopment, IdeaStatus.Completed, IdeaStatus.Archived },
        [IdeaStatus.ApprovedForDevelopment] = new() { IdeaStatus.InProgress, IdeaStatus.Archived },
        [IdeaStatus.InProgress] = new() { IdeaStatus.Completed, IdeaStatus.Archived },
        [IdeaStatus.Completed] = new() { IdeaStatus.Archived },
        [IdeaStatus.Rejected] = new() { IdeaStatus.Archived },
        [IdeaStatus.Withdrawn] = new() { IdeaStatus.Archived },
        [IdeaStatus.Archived] = new()
    };

    public static bool CanTransition(IdeaStatus from, IdeaStatus to) =>
        Allowed.TryGetValue(from, out var targets) && targets.Contains(to);
}


