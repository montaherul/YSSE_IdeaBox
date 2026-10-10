using Microsoft.EntityFrameworkCore;
using YSSE.IdeaBox.Application.Abstractions;
using YSSE.IdeaBox.Application.Abstractions.Services;
using YSSE.IdeaBox.Application.Common;
using YSSE.IdeaBox.Application.Dtos;
using YSSE.IdeaBox.Application.Services.Common;
using YSSE.IdeaBox.Domain.Entities;
using YSSE.IdeaBox.Domain.Enums;

namespace YSSE.IdeaBox.Application.Services.Ideas;

public sealed class ScreeningService : IScreeningService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _current;
    private readonly IDateTimeProvider _clock;
    private readonly IAuditWriter _audit;
    private readonly INotificationDispatcher _notifications;

    public ScreeningService(
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

    public async Task<PagedResult<IdeaListItemDto>> GetQueueAsync(IdeaListQuery query, CancellationToken ct = default)
    {
        if (!(_current.IsModeratorOrAdmin || _current.IsScreeningReviewer))
        {
            throw new ForbiddenException();
        }

        var (page, pageSize) = Pagination.Normalize(query.Page, query.PageSize);
        var q = _unitOfWork.Repository<Idea>().Query().Where(i => !i.IsDeleted &&
                                     (i.Status == IdeaStatus.Submitted || i.Status == IdeaStatus.UnderReview));

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var pattern = $"%{query.Search.Trim().ToLower()}%";
            q = q.Where(x => EF.Functions.Like(x.Title.ToLower(), pattern) ||
                             EF.Functions.Like(x.ShortSummary.ToLower(), pattern));
        }

        if (query.CategoryId is not null)
        {
            q = q.Where(x => x.CategoryId == query.CategoryId.Value);
        }

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderBy(x => x.SubmittedAt ?? x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(IdeaProjections.List)
            .ToListAsync(ct);

        return PagedResult<IdeaListItemDto>.Create(items, page, pageSize, total);
    }

    public async Task<Result> DecideAsync(int ideaId, ScreeningDecisionRequest request, CancellationToken ct = default)
    {
        if (!(_current.IsModeratorOrAdmin || _current.IsScreeningReviewer))
        {
            throw new ForbiddenException();
        }

        if (request.Decision is not (IdeaStatus.Published or IdeaStatus.Rejected or IdeaStatus.RevisionRequired))
        {
            return Result.Fail("A screening decision must be Published, Rejected or RevisionRequired.");
        }

        var idea = await _unitOfWork.Repository<Idea>().Query().FirstOrDefaultAsync(i => i.Id == ideaId && !i.IsDeleted, ct);
        if (idea is null)
        {
            throw new NotFoundException("Idea not found.");
        }

        if (idea.Status is not (IdeaStatus.Submitted or IdeaStatus.UnderReview))
        {
            return Result.Fail("Only submitted or under-review ideas can be screened.");
        }

        await _unitOfWork.Repository<IdeaScreeningDecision>().AddAsync(new IdeaScreeningDecision
        {
            IdeaId = idea.Id,
            ReviewerUserId = _current.RequireUserId(),
            Decision = request.Decision,
            OriginalityScore = request.OriginalityScore,
            FeasibilityScore = request.FeasibilityScore,
            RelevanceScore = request.RelevanceScore,
            ImpactScore = request.ImpactScore,
            Feedback = request.Feedback,
            CreatedAt = _clock.UtcNow
        }, ct);

        var from = idea.Status;
        await ApplyStatusAsync(idea, request.Decision, request.Feedback, ct);
        if (request.Decision == IdeaStatus.Published)
        {
            idea.PublishedAt = _clock.UtcNow;
        }

        idea.UpdatedAt = _clock.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        var type = request.Decision switch
        {
            IdeaStatus.Published => NotificationType.ScreeningDecision,
            IdeaStatus.RevisionRequired => NotificationType.RevisionRequested,
            _ => NotificationType.ScreeningDecision
        };
        await _notifications.NotifyAsync(idea.CreatorUserId, type,
            $"Your idea was {request.Decision.ToString().ToLowerInvariant()}",
            $"Your idea \"{idea.Title}\" screening result: {request.Decision}.",
            null, ct: ct);

await _audit.RecordAsync("ScreeningDecided", nameof(Idea), idea.Id.ToString(),
            request.Feedback, new { idea.CreatorUserId, from, to = request.Decision }, ct);

        return Result.Ok();
    }

    public async Task<Result> ChangeStatusAsync(int ideaId, StatusChangeRequest request, CancellationToken ct = default)
    {
        if (!(_current.IsModeratorOrAdmin || _current.IsScreeningReviewer))
        {
            throw new ForbiddenException();
        }

        var idea = await _unitOfWork.Repository<Idea>().Query().FirstOrDefaultAsync(i => i.Id == ideaId && !i.IsDeleted, ct);
        if (idea is null)
        {
            throw new NotFoundException("Idea not found.");
        }

        if (idea.Status == request.ToStatus)
        {
            return Result.Fail("The idea is already in that status.");
        }

        if (!IdeaStatusMachine.CanTransition(idea.Status, request.ToStatus))
        {
            return Result.Fail($"The transition from {idea.Status} to {request.ToStatus} is not allowed.");
        }

        var from = idea.Status;
        await ApplyStatusAsync(idea, request.ToStatus, request.Reason, ct);
        if (request.ToStatus == IdeaStatus.Published)
        {
            idea.PublishedAt = _clock.UtcNow;
        }

        idea.UpdatedAt = _clock.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        await _audit.RecordAsync("IdeaStatusChanged", nameof(Idea), idea.Id.ToString(),
            request.Reason, new { from, to = request.ToStatus }, ct);

        if (request.ToStatus == IdeaStatus.Published)
        {
            await _notifications.NotifyAsync(idea.CreatorUserId, NotificationType.ScreeningDecision,
                "Your idea was published", $"Your idea \"{idea.Title}\" is now public.", null, ct: ct);
        }

        return Result.Ok();
    }

    public async Task<IReadOnlyList<IdeaScreeningDecisionDto>> GetDecisionsAsync(int ideaId, CancellationToken ct = default)
    {
        var idea = await _unitOfWork.Repository<Idea>().Query().FirstOrDefaultAsync(i => i.Id == ideaId && !i.IsDeleted, ct);
        if (idea is null || (!_current.IsModeratorOrAdmin && !_current.IsScreeningReviewer && _current.UserId != idea.CreatorUserId))
        {
            throw new ForbiddenException();
        }

        return await _unitOfWork.Repository<IdeaScreeningDecision>().Query()
            .Where(d => d.IdeaId == ideaId)
            .OrderByDescending(d => d.CreatedAt)
            .Select(d => new IdeaScreeningDecisionDto
            {
                Id = d.Id,
                Decision = d.Decision,
                OriginalityScore = d.OriginalityScore,
                FeasibilityScore = d.FeasibilityScore,
                RelevanceScore = d.RelevanceScore,
                ImpactScore = d.ImpactScore,
                Feedback = d.Feedback,
                CreatedAt = d.CreatedAt
            })
            .ToListAsync(ct);
    }

    private async Task ApplyStatusAsync(Idea idea, IdeaStatus to, string? reason, CancellationToken ct)
    {
        await _unitOfWork.Repository<IdeaStatusHistory>().AddAsync(new IdeaStatusHistory
        {
            IdeaId = idea.Id,
            FromStatus = idea.Status,
            ToStatus = to,
            ActorUserId = _current.RequireUserId(),
            Reason = reason,
            CreatedAt = _clock.UtcNow
        }, ct);
        idea.Status = to;
    }
}


