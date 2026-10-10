using Microsoft.EntityFrameworkCore;
using YSSE.IdeaBox.Application.Abstractions;
using YSSE.IdeaBox.Application.Abstractions.Services;
using YSSE.IdeaBox.Application.Common;
using YSSE.IdeaBox.Application.Dtos;
using YSSE.IdeaBox.Domain.Entities;
using YSSE.IdeaBox.Domain.Enums;

using YSSE.IdeaBox.Application.Services.Common;

namespace YSSE.IdeaBox.Application.Services.Engagement;

public sealed class MessagingService : IMessagingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _current;
    private readonly IDateTimeProvider _clock;
    private readonly IAuditWriter _audit;
    private readonly INotificationDispatcher _notifications;

    public MessagingService(
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

    public async Task<PagedResult<ConversationListItemDto>> GetConversationsAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var userId = _current.RequireUserId();
        var (p, s) = Pagination.Normalize(page, pageSize);

        var q = _unitOfWork.Repository<Conversation>().Query()
            .Where(c => c.Participants.Any(pp => pp.UserId == userId))
            .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt);

        var total = await q.CountAsync(ct);
        var conversations = await q
            .Skip((p - 1) * s)
            .Take(s)
            .Include(c => c.Participants)
            .Include(c => c.Messages)
            .ToListAsync(ct);

        var otherUserIds = conversations
            .SelectMany(c => c.Participants.Where(pp => pp.UserId != userId).Select(pp => pp.UserId))
            .Distinct()
            .ToList();
        var users = await LoadNamesAsync(otherUserIds, ct);

        var ideaIds = conversations.Where(c => c.IdeaId is not null).Select(c => c.IdeaId!.Value).Distinct().ToList();
        var ideas = await _unitOfWork.Repository<Idea>().Query()
            .Where(i => ideaIds.Contains(i.Id))
            .Select(i => new { i.Id, i.Title, i.Attribution, i.CreatorUserId })
            .ToDictionaryAsync(i => i.Id, ct);
        var ideasWithTitle = conversations
            .Select(c => new { c.Id, Title = ideas.TryGetValue(c.IdeaId ?? 0, out var id) ? id.Title : null, Attribute = ideas.TryGetValue(c.IdeaId ?? 0, out var i2) ? i2.Attribution : (IdeaAttribution?)null, CreatorUserId = ideas.TryGetValue(c.IdeaId ?? 0, out var i3) ? i3.CreatorUserId : 0 })
            .ToDictionary(x => x.Id);

        var items = conversations.Select(c =>
        {
            var counterpartyId = c.Participants.FirstOrDefault(pp => pp.UserId != userId)?.UserId ?? userId;
            var ideaRef = ideasWithTitle[c.Id];
            var anonymousCreator = ideaRef.Attribute == IdeaAttribution.Anonymous &&
                                   ideaRef.CreatorUserId == counterpartyId;

            var lastMessage = c.Messages.Where(m => !m.IsDeleted)
                .OrderByDescending(m => m.CreatedAt)
                .FirstOrDefault();

            var unread = c.Messages.Count(m => m.SenderUserId != userId && m.ReadAt == null && !m.IsDeleted);

            return new ConversationListItemDto
            {
                Id = c.Id,
                IdeaId = c.IdeaId,
                IdeaTitle = ideaRef.Title,
                Status = c.Status,
                CounterpartyName = anonymousCreator
                    ? (users.TryGetValue(counterpartyId, out var alias) && alias != null ? alias : "Anonymous")
                    : (users.TryGetValue(counterpartyId, out var display) && display != null ? display : "Member"),
                LastMessagePreview = lastMessage?.Body,
                LastMessageAt = c.LastMessageAt ?? lastMessage?.CreatedAt,
                UnreadCount = unread
            };
        }).ToList();

        return PagedResult<ConversationListItemDto>.Create(items, p, s, total);
    }

    public async Task<ConversationDetailDto?> GetConversationAsync(int id, int page, int pageSize, CancellationToken ct = default)
    {
        var userId = _current.RequireUserId();
        var (p, s) = Pagination.Normalize(page, pageSize);

        var conversation = await _unitOfWork.Repository<Conversation>().Query()
            .Include(c => c.Participants)
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        if (conversation is null || !conversation.Participants.Any(pp => pp.UserId == userId))
        {
            throw new NotFoundException("Conversation not found.");
        }

        var senderIds = conversation.Messages.Select(m => m.SenderUserId).Distinct().ToList();
        var users = await LoadNamesAsync(senderIds, ct);

        var messages = conversation.Messages
            .Where(m => !m.IsDeleted)
            .OrderByDescending(m => m.CreatedAt)
            .Skip((p - 1) * s)
            .Take(s)
            .OrderBy(m => m.CreatedAt)
            .Select(m => new MessageDto
            {
                Id = m.Id,
                SenderUserId = m.SenderUserId,
                SenderName = users.TryGetValue(m.SenderUserId, out var name) && name != null ? name : "Member",
                IsMine = m.SenderUserId == userId,
                Body = m.Body,
                DeliveredAt = m.DeliveredAt,
                ReadAt = m.ReadAt,
                CreatedAt = m.CreatedAt
            })
            .ToList();

        return new ConversationDetailDto
        {
            Id = conversation.Id,
            IdeaId = conversation.IdeaId,
            Status = conversation.Status,
            Messages = messages
        };
    }

    public async Task<Result<int>> StartAsync(StartConversationRequest request, CancellationToken ct = default)
    {
        var userId = _current.RequireUserId();
        var idea = await _unitOfWork.Repository<Idea>().Query().FirstOrDefaultAsync(i => i.Id == request.IdeaId && !i.IsDeleted, ct);
        if (idea is null)
        {
            throw new NotFoundException("Idea not found.");
        }

        if (idea.CreatorUserId != userId)
        {
            var approved = await _unitOfWork.Repository<EntrepreneurProfile>().Query().AnyAsync(e => e.UserId == userId && e.IsApproved, ct);
            if (!approved)
            {
                throw new ForbiddenException("Only approved opportunity seekers can start a conversation.");
            }
        }

        if (await IsBlockedAsync(userId, idea.CreatorUserId, ct) ||
            await IsBlockedAsync(idea.CreatorUserId, userId, ct))
        {
            throw new ForbiddenException("Messaging is blocked with this user.");
        }

        var existing = await _unitOfWork.Repository<Conversation>().Query()
            .Where(c => c.IdeaId == idea.Id &&
                        c.Participants.Any(pp => pp.UserId == userId) &&
                        c.Participants.Any(pp => pp.UserId == idea.CreatorUserId))
            .Select(c => (int?)c.Id)
            .FirstOrDefaultAsync(ct);
        if (existing is not null)
        {
            return Result<int>.Ok(existing.Value);
        }

        var conversation = new Conversation
        {
            IdeaId = idea.Id,
            Status = ConversationStatus.Open,
            CreatedAt = _clock.UtcNow
        };
        await _unitOfWork.Repository<Conversation>().AddAsync(conversation, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        foreach (var participant in new[]
        {
            new ConversationParticipant { ConversationId = conversation.Id, UserId = userId, CreatedAt = _clock.UtcNow },
            new ConversationParticipant { ConversationId = conversation.Id, UserId = idea.CreatorUserId, CreatedAt = _clock.UtcNow }
        })
        {
            await _unitOfWork.Repository<ConversationParticipant>().AddAsync(participant, ct);
        }
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<int>.Ok(conversation.Id);
    }

    public async Task<Result<MessageDto>> SendMessageAsync(int conversationId, MessageSendRequest request, CancellationToken ct = default)
    {
        var userId = _current.RequireUserId();
        var conversation = await _unitOfWork.Repository<Conversation>().Query()
            .Include(c => c.Participants)
            .FirstOrDefaultAsync(c => c.Id == conversationId, ct);

        if (conversation is null || !conversation.Participants.Any(pp => pp.UserId == userId))
        {
            throw new NotFoundException("Conversation not found.");
        }

        if (conversation.Status == ConversationStatus.Blocked)
        {
            throw new ForbiddenException("This conversation is blocked.");
        }

        if (conversation.Status == ConversationStatus.Closed)
        {
            return Result<MessageDto>.Fail("This conversation is closed.");
        }

        if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
        {
            var existing = await _unitOfWork.Repository<Message>().Query().FirstOrDefaultAsync(
                m => m.ConversationId == conversationId && m.SenderUserId == userId && m.IdempotencyKey == request.IdempotencyKey, ct);
            if (existing is not null)
            {
                return Result<MessageDto>.Ok(ToMessageDto(existing, userId, ""));
            }
        }

        var message = new Message
        {
            ConversationId = conversationId,
            SenderUserId = userId,
            Body = request.Body,
            DeliveredAt = _clock.UtcNow,
            IdempotencyKey = request.IdempotencyKey,
            CreatedAt = _clock.UtcNow
        };
        await _unitOfWork.Repository<Message>().AddAsync(message, ct);
        conversation.LastMessageAt = _clock.UtcNow;
        conversation.UpdatedAt = _clock.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        var counterpartyId = conversation.Participants.FirstOrDefault(pp => pp.UserId != userId)?.UserId;
        if (counterpartyId is not null)
        {
            await _notifications.NotifyAsync(counterpartyId.Value, NotificationType.NewMessage,
                "New message", "You received a new message.", null, ct: ct);
        }

        return Result<MessageDto>.Ok(ToMessageDto(message, userId, ""));
    }

    public async Task<Result> MarkReadAsync(int conversationId, CancellationToken ct = default)
    {
        var userId = _current.RequireUserId();
        var conversation = await _unitOfWork.Repository<Conversation>().Query()
            .Include(c => c.Participants)
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == conversationId, ct);

        if (conversation is null || !conversation.Participants.Any(pp => pp.UserId == userId))
        {
            throw new NotFoundException("Conversation not found.");
        }

        var participant = conversation.Participants.First(pp => pp.UserId == userId);
        participant.LastReadAt = _clock.UtcNow;

        foreach (var message in conversation.Messages.Where(m => m.SenderUserId != userId && m.ReadAt == null))
        {
            message.ReadAt = _clock.UtcNow;
        }

        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Ok();
    }

    public async Task<Result> SetStatusAsync(int conversationId, ConversationStatus status, CancellationToken ct = default)
    {
        var userId = _current.RequireUserId();
        var conversation = await _unitOfWork.Repository<Conversation>().Query()
            .Include(c => c.Participants)
            .FirstOrDefaultAsync(c => c.Id == conversationId, ct);

        if (conversation is null || !conversation.Participants.Any(pp => pp.UserId == userId))
        {
            throw new NotFoundException("Conversation not found.");
        }

        conversation.Status = status;
        conversation.UpdatedAt = _clock.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Ok();
    }

    public async Task<Result> BlockUserAsync(int userId, CancellationToken ct = default)
    {
        var blockerId = _current.RequireUserId();
        if (blockerId == userId)
        {
            return Result.Fail("You cannot block yourself.");
        }

        var existing = await _unitOfWork.Repository<UserBlock>().Query().FirstOrDefaultAsync(
            b => b.BlockerUserId == blockerId && b.BlockedUserId == userId, ct);
        if (existing is not null)
        {
            return Result.Fail("This user is already blocked.");
        }

        await _unitOfWork.Repository<UserBlock>().AddAsync(new UserBlock
        {
            BlockerUserId = blockerId,
            BlockedUserId = userId,
            CreatedAt = _clock.UtcNow
        }, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var blockedConversations = await _unitOfWork.Repository<Conversation>().Query()
            .Where(c => c.Participants.Any(pp => pp.UserId == blockerId) &&
                        c.Participants.Any(pp => pp.UserId == userId) &&
                        c.Status != ConversationStatus.Blocked)
            .ToListAsync(ct);
        foreach (var conversation in blockedConversations)
        {
            conversation.Status = ConversationStatus.Blocked;
        }

        await _unitOfWork.SaveChangesAsync(ct);
        await _audit.RecordAsync("UserBlocked", nameof(UserBlock), null, null, new { blockedUserId = userId }, ct);
        return Result.Ok();
    }

    public async Task<Result> ReportAsync(ReportCreateRequest request, CancellationToken ct = default)
    {
        var reporterId = _current.RequireUserId();
        await _unitOfWork.Repository<Report>().AddAsync(new Report
        {
            TargetType = request.TargetType,
            TargetId = request.TargetId,
            ReporterUserId = reporterId,
            Reason = request.Reason,
            Details = request.Details,
            Status = ReportStatus.Open,
            CreatedAt = _clock.UtcNow
        }, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Ok();
    }

    private async Task<bool> IsBlockedAsync(int blockerId, int blockedId, CancellationToken ct) =>
        await _unitOfWork.Repository<UserBlock>().Query().AnyAsync(b => b.BlockerUserId == blockerId && b.BlockedUserId == blockedId, ct);

    /// <summary>Display name per user, plus alias for anonymous creators.</summary>
    private async Task<Dictionary<int, string?>> LoadNamesAsync(IEnumerable<int> userIds, CancellationToken ct)
    {
        var ids = userIds.Distinct().ToList();
        if (ids.Count == 0)
        {
            return new Dictionary<int, string?>();
        }

        var users = await _unitOfWork.Repository<User>().Query()
            .Where(u => ids.Contains(u.Id))
            .Select(u => new { u.Id, u.DisplayName, Alias = u.CreatorProfile != null && !u.CreatorProfile.IsDeleted ? u.CreatorProfile.Alias : (string?)null })
            .ToListAsync(ct);

        return users.ToDictionary(u => u.Id, u => (string?)(u.Alias ?? u.DisplayName));
    }

    private static MessageDto ToMessageDto(Message message, int userId, string senderName) => new()
    {
        Id = message.Id,
        SenderUserId = message.SenderUserId,
        SenderName = senderName,
        IsMine = message.SenderUserId == userId,
        Body = message.Body,
        DeliveredAt = message.DeliveredAt,
        ReadAt = message.ReadAt,
        CreatedAt = message.CreatedAt
    };
}


