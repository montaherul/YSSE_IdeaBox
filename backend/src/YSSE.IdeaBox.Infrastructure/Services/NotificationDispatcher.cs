using Microsoft.EntityFrameworkCore;
using YSSE.IdeaBox.Application.Abstractions;
using YSSE.IdeaBox.Domain.Entities;
using YSSE.IdeaBox.Domain.Enums;

namespace YSSE.IdeaBox.Infrastructure.Services;

/// <summary>
/// Persists in-app notifications, honouring the recipient's preferences for
/// non-essential notifications. Essential (security/account) notices always persist.
/// </summary>
public sealed class NotificationDispatcher : INotificationDispatcher
{
    private readonly IAppDbContext _db;
    private readonly IDateTimeProvider _clock;

    public NotificationDispatcher(IAppDbContext db, IDateTimeProvider clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task NotifyAsync(int userId, NotificationType type, string title, string body,
        string? linkUrl = null, bool essential = false, CancellationToken ct = default)
    {
        if (!essential)
        {
            var disabled = await _db.NotificationPreferences.AnyAsync(
                p => p.UserId == userId && p.Type == type && p.Channel == NotificationChannel.InApp && !p.IsEnabled,
                ct);

            if (disabled)
            {
                return;
            }
        }

        await _db.Notifications.AddAsync(new Notification
        {
            UserId = userId,
            Type = type,
            Title = title,
            Body = body,
            LinkUrl = linkUrl,
            IsEssential = essential,
            IsRead = false,
            CreatedAt = _clock.UtcNow
        }, ct);

        await _db.SaveChangesAsync(ct);
    }
}
