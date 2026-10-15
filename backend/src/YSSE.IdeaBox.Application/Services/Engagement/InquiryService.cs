using Microsoft.EntityFrameworkCore;
using YSSE.IdeaBox.Application.Abstractions;
using YSSE.IdeaBox.Application.Abstractions.Services;
using YSSE.IdeaBox.Application.Common;
using YSSE.IdeaBox.Application.Dtos;
using YSSE.IdeaBox.Application.Services.Common;
using YSSE.IdeaBox.Domain.Entities;
using YSSE.IdeaBox.Domain.Enums;

namespace YSSE.IdeaBox.Application.Services.Engagement;

public sealed class InquiryService : IInquiryService
{
    private const int DefaultPerSeekerPerDayLimit = 10;
    private const int DefaultPerIdeaLimit = 1;

    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _current;
    private readonly IDateTimeProvider _clock;
    private readonly IAuditWriter _audit;
    private readonly INotificationDispatcher _notifications;

    public InquiryService(
        IUnitOfWork db,
        ICurrentUserService current,
        IDateTimeProvider clock,
        IAuditWriter audit,
        INotificationDispatcher notifications)
    {
        _unitOfWork = db;
        _current = current;
        _clock = clock;
        _audit = audit;
        _notifications = notifications;
    }

    public async Task<Result<int>> SendAsync(int ideaId, InquiryCreateRequest request, CancellationToken ct = default)
    {
        var userId = _current.RequireUserId();

        var seeker = await _unitOfWork.Repository<EntrepreneurProfile>().Query().FirstOrDefaultAsync(e => e.UserId == userId && e.IsApproved, ct);
        if (seeker is null)
        {
            throw new ForbiddenException("Only approved opportunity seekers can send inquiries.");
        }

        var idea = await _unitOfWork.Repository<Idea>().Query().FirstOrDefaultAsync(i => i.Id == ideaId && !i.IsDeleted, ct);
        if (idea is null || idea.Status != IdeaStatus.Published)
        {
            throw new NotFoundException("Idea not found.");
        }

        if (idea.CreatorUserId == userId)
        {
            throw new ForbiddenException("You cannot send an inquiry about your own idea.");
        }

        if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
        {
            var existing = await _unitOfWork.Repository<EntrepreneurInquiry>().Query().FirstOrDefaultAsync(
                i => i.SeekerUserId == userId && i.IdempotencyKey == request.IdempotencyKey, ct);
            if (existing is not null)
            {
                return Result<int>.Ok(existing.Id);
            }
        }

        var dayStart = _clock.UtcNow.Date;
        var perDayLimit = await GetIntSettingAsync("inquiry.daily.limit", DefaultPerSeekerPerDayLimit, ct);
        var sentToday = await _unitOfWork.Repository<EntrepreneurInquiry>().Query().CountAsync(
            i => i.SeekerUserId == userId && i.CreatedAt >= dayStart, ct);
        if (sentToday >= perDayLimit)
        {
            return Result<int>.Fail($"You have reached the daily limit of {perDayLimit} inquiries.");
        }

        var perIdeaLimit = await GetIntSettingAsync("Inquiry.PerIdeaLimit", DefaultPerIdeaLimit, ct);
        if (perIdeaLimit > 0)
        {
            var sentForIdea = await _unitOfWork.Repository<EntrepreneurInquiry>().Query().CountAsync(
                i => i.SeekerUserId == userId && i.IdeaId == ideaId, ct);
            if (sentForIdea >= perIdeaLimit)
            {
                return Result<int>.Fail("You have already sent an inquiry about this idea.");
            }
        }

        var inquiry = new EntrepreneurInquiry
        {
            IdeaId = ideaId,
            SeekerUserId = userId,
            Type = request.Type,
            Message = request.Message,
            IdempotencyKey = request.IdempotencyKey,
            CreatedAt = _clock.UtcNow
        };
        await _unitOfWork.Repository<EntrepreneurInquiry>().AddAsync(inquiry, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        await _notifications.NotifyAsync(idea.CreatorUserId, NotificationType.NewInquiry,
            "New interest in your idea",
            $"{seeker.Organization} sent an inquiry about \"{idea.Title}\".", null, ct: ct);

        return Result<int>.Ok(inquiry.Id);
    }

    public async Task<PagedResult<InquiryDto>> GetReceivedAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var userId = _current.RequireUserId();
        var (p, s) = Pagination.Normalize(page, pageSize);

        var q = _unitOfWork.Repository<EntrepreneurInquiry>().Query().Where(i => i.Idea.CreatorUserId == userId);
        var total = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(i => i.CreatedAt)
            .Skip((p - 1) * s)
            .Take(s)
            .Select(i => new InquiryDto
            {
                Id = i.Id,
                IdeaId = i.IdeaId,
                IdeaTitle = i.Idea.Title,
                SeekerName = i.Seeker.DisplayName,
                Type = i.Type,
                Message = i.Message,
                IsDeclined = i.IsDeclined,
                IsAnswered = i.IsAnswered,
                ConversationId = i.ConversationId,
                CreatedAt = i.CreatedAt
            })
            .ToListAsync(ct);

        return PagedResult<InquiryDto>.Create(items, p, s, total);
    }

    public async Task<PagedResult<InquiryDto>> GetSentAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var userId = _current.RequireUserId();
        var (p, s) = Pagination.Normalize(page, pageSize);

        var q = _unitOfWork.Repository<EntrepreneurInquiry>().Query().Where(i => i.SeekerUserId == userId);
        var total = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(i => i.CreatedAt)
            .Skip((p - 1) * s)
            .Take(s)
            .Select(i => new InquiryDto
            {
                Id = i.Id,
                IdeaId = i.IdeaId,
                IdeaTitle = i.Idea.Title,
                SeekerName = i.Seeker.DisplayName,
                Type = i.Type,
                Message = i.Message,
                IsDeclined = i.IsDeclined,
                IsAnswered = i.IsAnswered,
                ConversationId = i.ConversationId,
                CreatedAt = i.CreatedAt
            })
            .ToListAsync(ct);

        return PagedResult<InquiryDto>.Create(items, p, s, total);
    }

    public async Task<Result<ConversationRef>> RespondAsync(int inquiryId, InquiryRespondRequest request, CancellationToken ct = default)
    {
        var userId = _current.RequireUserId();
        var inquiry = await _unitOfWork.Repository<EntrepreneurInquiry>().Query().FirstOrDefaultAsync(i => i.Id == inquiryId, ct);
        if (inquiry is null)
        {
            throw new NotFoundException("Inquiry not found.");
        }

        var idea = await _unitOfWork.Repository<Idea>().Query().FirstOrDefaultAsync(i => i.Id == inquiry.IdeaId && !i.IsDeleted, ct);
        if (idea is null || idea.CreatorUserId != userId)
        {
            throw new ForbiddenException("Only the idea creator can respond to this inquiry.");
        }

        inquiry.IsAnswered = true;
        inquiry.AnsweredAt = _clock.UtcNow;
        inquiry.UpdatedAt = _clock.UtcNow;

        if (request.Decline)
        {
            inquiry.IsDeclined = true;
            await _unitOfWork.SaveChangesAsync(ct);
            await _notifications.NotifyAsync(inquiry.SeekerUserId, NotificationType.CollaborationResponse,
                "Inquiry declined", "The creator declined your inquiry.", null, ct: ct);
            return Result<ConversationRef>.Ok(null!, "Inquiry declined.");
        }

        var conversation = new Conversation
        {
            IdeaId = inquiry.IdeaId,
            Status = ConversationStatus.Open,
            CreatedAt = _clock.UtcNow
        };
        await _unitOfWork.Repository<Conversation>().AddAsync(conversation, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        foreach (var participant in new[]
        {
            new ConversationParticipant { ConversationId = conversation.Id, UserId = userId, CreatedAt = _clock.UtcNow },
            new ConversationParticipant { ConversationId = conversation.Id, UserId = inquiry.SeekerUserId, CreatedAt = _clock.UtcNow }
        })
        {
            await _unitOfWork.Repository<ConversationParticipant>().AddAsync(participant, ct);
        }

        if (!string.IsNullOrWhiteSpace(request.Reply))
        {
            await _unitOfWork.Repository<Message>().AddAsync(new Message
            {
                ConversationId = conversation.Id,
                SenderUserId = userId,
                Body = request.Reply,
                DeliveredAt = _clock.UtcNow,
                CreatedAt = _clock.UtcNow
            }, ct);
            conversation.LastMessageAt = _clock.UtcNow;
        }

        inquiry.ConversationId = conversation.Id;
        await _unitOfWork.SaveChangesAsync(ct);

        await _audit.RecordAsync("InquiryResponded", nameof(EntrepreneurInquiry), inquiry.Id.ToString(),
            null, new { conversationId = conversation.Id, request.Decline }, ct);

        await _notifications.NotifyAsync(inquiry.SeekerUserId, NotificationType.CollaborationResponse,
            "The creator responded", $"The creator of \"{idea.Title}\" responded to your inquiry.", null, ct: ct);

        return Result<ConversationRef>.Ok(new ConversationRef(conversation.Id));
    }

    public async Task<Result> UpdateAsync(int inquiryId, InquiryUpdateRequest request, CancellationToken ct = default)
    {
        var userId = _current.RequireUserId();
        var inquiry = await _unitOfWork.Repository<EntrepreneurInquiry>().Query().FirstOrDefaultAsync(i => i.Id == inquiryId, ct);
        if (inquiry is null)
        {
            throw new NotFoundException("Inquiry not found.");
        }

        if (inquiry.SeekerUserId != userId)
        {
            throw new ForbiddenException("Only the sender can edit this inquiry.");
        }

        if (inquiry.IsAnswered || inquiry.IsDeclined || inquiry.ConversationId is not null)
        {
            return Result.Fail("This inquiry can no longer be edited because it has been answered.");
        }

        inquiry.Type = request.Type;
        inquiry.Message = request.Message;
        inquiry.UpdatedAt = _clock.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Ok();
    }

    public async Task<Result> DeleteAsync(int inquiryId, CancellationToken ct = default)
    {
        var userId = _current.RequireUserId();
        var inquiry = await _unitOfWork.Repository<EntrepreneurInquiry>().Query().FirstOrDefaultAsync(i => i.Id == inquiryId, ct);
        if (inquiry is null)
        {
            throw new NotFoundException("Inquiry not found.");
        }

        var isOwner = inquiry.SeekerUserId == userId;
        if (!isOwner && !_current.IsModeratorOrAdmin)
        {
            throw new ForbiddenException();
        }

        if (isOwner && !_current.IsModeratorOrAdmin && (inquiry.IsAnswered || inquiry.ConversationId is not null))
        {
            return Result.Fail("An answered inquiry cannot be withdrawn.");
        }

        var ideaId = inquiry.IdeaId;
        var seekerUserId = inquiry.SeekerUserId;
        _unitOfWork.Repository<EntrepreneurInquiry>().Remove(inquiry);
        await _unitOfWork.SaveChangesAsync(ct);

        await _audit.RecordAsync("InquiryDeleted", nameof(EntrepreneurInquiry), inquiryId.ToString(),
            null, new { ideaId, seekerUserId }, ct);
        return Result.Ok();
    }

    public async Task<PagedResult<IdeaListItemDto>> GetWatchlistAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var userId = _current.RequireUserId();
        var (p, s) = Pagination.Normalize(page, pageSize);

        var q = _unitOfWork.Repository<WatchlistItem>().Query().Where(w => w.UserId == userId);
        var total = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(w => w.CreatedAt)
            .Skip((p - 1) * s)
            .Take(s)
            .Select(w => w.Idea)
            .Select(IdeaProjections.List)
            .ToListAsync(ct);

        return PagedResult<IdeaListItemDto>.Create(items, p, s, total);
    }

    public async Task<Result> AddWatchlistAsync(int ideaId, CancellationToken ct = default)
    {
        var userId = _current.RequireUserId();
        var idea = await _unitOfWork.Repository<Idea>().Query().FirstOrDefaultAsync(i => i.Id == ideaId && !i.IsDeleted, ct);
        if (idea is null)
        {
            throw new NotFoundException("Idea not found.");
        }

        var exists = await _unitOfWork.Repository<WatchlistItem>().Query().AnyAsync(w => w.UserId == userId && w.IdeaId == ideaId, ct);
        if (exists)
        {
            return Result.Fail("This idea is already on your watchlist.");
        }

        await _unitOfWork.Repository<WatchlistItem>().AddAsync(new WatchlistItem { UserId = userId, IdeaId = ideaId, CreatedAt = _clock.UtcNow }, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Ok();
    }

    public async Task<Result> RemoveWatchlistAsync(int ideaId, CancellationToken ct = default)
    {
        var userId = _current.RequireUserId();
        var item = await _unitOfWork.Repository<WatchlistItem>().Query().FirstOrDefaultAsync(w => w.UserId == userId && w.IdeaId == ideaId, ct);
        if (item is null)
        {
            return Result.Fail("This idea is not on your watchlist.");
        }

        _unitOfWork.Repository<WatchlistItem>().Remove(item);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Ok();
    }

    public async Task<IReadOnlyList<MatchResultDto>> GetMatchesAsync(int take = 10, CancellationToken ct = default)
    {
        var userId = _current.RequireUserId();
        var seeker = await _unitOfWork.Repository<EntrepreneurProfile>().Query().FirstOrDefaultAsync(e => e.UserId == userId && e.IsApproved, ct);
        if (seeker is null)
        {
            throw new ForbiddenException("Only approved opportunity seekers can access matches.");
        }

        take = Math.Clamp(take, 1, 20);

        var ideas = await _unitOfWork.Repository<Idea>().Query()
            .Include(i => i.Category)
            .Include(i => i.IdeaTags).ThenInclude(t => t.Tag)
            .Where(i => !i.IsDeleted && i.Status == IdeaStatus.Published)
            .OrderByDescending(i => i.PublishedAt ?? i.CreatedAt)
            .Take(200)
            .ToListAsync(ct);

var matches = new List<MatchResultDto>();
        foreach (var idea in ideas)
        {
            var (score, reasons) = MatchingScorer.ScoreIdea(
                idea.Category?.Name,
                string.Join(",", idea.IdeaTags.Select(t => t.Tag.Name)),
                idea.SkillsNeeded,
                idea.Stage,
                seeker.Industry,
                seeker.InterestAreas,
                seeker.Expertise);

            if (score <= 0)
            {
                continue;
            }

            matches.Add(new MatchResultDto
            {
                IdeaId = idea.Id,
                Title = idea.Title,
                Slug = idea.Slug,
                CategoryName = idea.Category?.Name ?? string.Empty,
                Score = score,
                Reasons = reasons
            });
        }

        return matches
            .OrderByDescending(m => m.Score)
            .ThenBy(m => m.IdeaId)
            .Take(take)
            .ToList();
}

    private async Task<int> GetIntSettingAsync(string key, int defaultValue, CancellationToken ct)
    {
        var value = await _unitOfWork.Repository<PlatformSetting>().Query().Where(s => s.Key == key).Select(s => s.Value).FirstOrDefaultAsync(ct);
        return int.TryParse(value, out var parsed) ? parsed : defaultValue;
    }
}


