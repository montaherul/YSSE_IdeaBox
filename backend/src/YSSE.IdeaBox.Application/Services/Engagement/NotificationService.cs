using Microsoft.EntityFrameworkCore;
using YSSE.IdeaBox.Application.Abstractions;
using YSSE.IdeaBox.Application.Abstractions.Services;
using YSSE.IdeaBox.Application.Common;
using YSSE.IdeaBox.Application.Dtos;
using YSSE.IdeaBox.Domain.Entities;
using YSSE.IdeaBox.Domain.Enums;

using YSSE.IdeaBox.Application.Services.Common;

namespace YSSE.IdeaBox.Application.Services.Engagement;

public sealed class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _current;
    private readonly IDateTimeProvider _clock;

    public NotificationService(IUnitOfWork db, ICurrentUserService current, IDateTimeProvider clock)
    {
        _unitOfWork = db;
        _current = current;
        _clock = clock;
    }

    public async Task<PagedResult<NotificationDto>> GetMineAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var userId = _current.RequireUserId();
        var (p, s) = Pagination.Normalize(page, pageSize);

        var q = _unitOfWork.Repository<Notification>().Query().Where(n => n.UserId == userId).OrderByDescending(n => n.CreatedAt);
        var total = await q.CountAsync(ct);

        var items = await q
            .Skip((p - 1) * s)
            .Take(s)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                Type = n.Type,
                Title = n.Title,
                Body = n.Body,
                LinkUrl = n.LinkUrl,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            })
            .ToListAsync(ct);

        return PagedResult<NotificationDto>.Create(items, p, s, total);
    }

    public async Task<Result> MarkReadAsync(int id, CancellationToken ct = default)
    {
        var userId = _current.RequireUserId();
        var notification = await _unitOfWork.Repository<Notification>().Query().FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId, ct);
        if (notification is null)
        {
            throw new NotFoundException("Notification not found.");
        }

        notification.IsRead = true;
        notification.ReadAt = _clock.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Ok();
    }

    public async Task<Result> MarkAllReadAsync(CancellationToken ct = default)
    {
        var userId = _current.RequireUserId();
        var unread = await _unitOfWork.Repository<Notification>().Query().Where(n => n.UserId == userId && !n.IsRead).ToListAsync(ct);
        var now = _clock.UtcNow;
        foreach (var notification in unread)
        {
            notification.IsRead = true;
            notification.ReadAt = now;
        }

        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Ok();
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
    {
        var userId = _current.RequireUserId();
        var notification = await _unitOfWork.Repository<Notification>().Query().FirstOrDefaultAsync(n => n.Id == id, ct);
        if (notification is null)
        {
            throw new NotFoundException("Notification not found.");
        }

        if (notification.UserId != userId && !_current.IsModeratorOrAdmin)
        {
            throw new ForbiddenException();
        }

        _unitOfWork.Repository<Notification>().Remove(notification);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Ok();
    }

    public async Task<IReadOnlyList<NotificationPreferenceDto>> GetPreferencesAsync(CancellationToken ct = default)
    {
        var userId = _current.RequireUserId();
        var stored = await _unitOfWork.Repository<NotificationPreference>().Query()
            .Where(p => p.UserId == userId)
            .ToListAsync(ct);

        var overrides = stored.ToDictionary(p => (p.Type, p.Channel), p => p.IsEnabled);
        var result = new List<NotificationPreferenceDto>();

        foreach (var type in Enum.GetValues<NotificationType>())
        {
            foreach (var channel in Enum.GetValues<NotificationChannel>())
            {
                var enabled = overrides.TryGetValue((type, channel), out var value) ? value : true;
                result.Add(new NotificationPreferenceDto(type, channel, enabled));
            }
        }

        return result;
    }

    public async Task<Result> UpdatePreferenceAsync(NotificationPreferenceUpdateRequest request, CancellationToken ct = default)
    {
        var userId = _current.RequireUserId();

        if (request.Type == NotificationType.AccountNotice && !request.IsEnabled)
        {
            return Result.Fail("Security and account notices cannot be disabled.");
        }

        var preference = await _unitOfWork.Repository<NotificationPreference>().Query().FirstOrDefaultAsync(
            p => p.UserId == userId && p.Type == request.Type && p.Channel == request.Channel, ct);

        if (preference is null)
        {
            await _unitOfWork.Repository<NotificationPreference>().AddAsync(new NotificationPreference
            {
                UserId = userId,
                Type = request.Type,
                Channel = request.Channel,
                IsEnabled = request.IsEnabled
            }, ct);
        }
        else
        {
            preference.IsEnabled = request.IsEnabled;
        }

        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Ok();
    }
}


