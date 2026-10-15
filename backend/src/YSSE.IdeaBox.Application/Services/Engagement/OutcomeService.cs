using Microsoft.EntityFrameworkCore;
using YSSE.IdeaBox.Application.Abstractions;
using YSSE.IdeaBox.Application.Abstractions.Services;
using YSSE.IdeaBox.Application.Common;
using YSSE.IdeaBox.Application.Dtos;
using YSSE.IdeaBox.Domain.Entities;
using YSSE.IdeaBox.Domain.Enums;

using YSSE.IdeaBox.Application.Services.Common;

namespace YSSE.IdeaBox.Application.Services.Engagement;

public sealed class OutcomeService : IOutcomeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _current;
    private readonly IDateTimeProvider _clock;
    private readonly IAuditWriter _audit;
    private readonly INotificationDispatcher _notifications;

    public OutcomeService(
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

    public async Task<PagedResult<OutcomeDto>> GetForIdeaAsync(int ideaId, int page, int pageSize, CancellationToken ct = default)
    {
        var (p, s) = Pagination.Normalize(page, pageSize);
        var userId = _current.UserId;
        var q = _unitOfWork.Repository<OpportunityOutcome>().Query().Where(o => o.IdeaId == ideaId);

        if (!_current.IsAdmin)
        {
            if (userId is null || !await IsParticipantAsync(ideaId, userId.Value, ct))
            {
                q = q.Where(o => o.IsPublic);
            }
        }

        var total = await q.CountAsync(ct);

        var items = await q
            .OrderByDescending(o => o.CreatedAt)
            .Skip((p - 1) * s)
            .Take(s)
            .Select(o => new OutcomeDto
            {
                Id = o.Id,
                IdeaId = o.IdeaId,
                Kind = o.Kind,
                Status = o.Status,
                Description = o.Description,
                IsVerified = o.IsVerified,
                IsPublic = o.IsPublic,
                CreatedAt = o.CreatedAt
            })
            .ToListAsync(ct);

        return PagedResult<OutcomeDto>.Create(items, p, s, total);
    }

    public async Task<Result<int>> CreateAsync(OutcomeCreateRequest request, CancellationToken ct = default)
    {
        var userId = _current.RequireUserId();
        var idea = await _unitOfWork.Repository<Idea>().Query().FirstOrDefaultAsync(i => i.Id == request.IdeaId && !i.IsDeleted, ct);
        if (idea is null)
        {
            throw new NotFoundException("Idea not found.");
        }

        var isCreator = idea.CreatorUserId == userId;
        var isSeeker = await _unitOfWork.Repository<EntrepreneurInquiry>().Query().AnyAsync(i => i.IdeaId == idea.Id && i.SeekerUserId == userId, ct);
        if (!isCreator && !isSeeker && !_current.IsAdmin)
        {
            throw new ForbiddenException("Only participants in this idea can report an outcome.");
        }

        var outcome = new OpportunityOutcome
        {
            IdeaId = idea.Id,
            ReporterUserId = userId,
            Kind = request.Kind,
            Status = request.Status,
            Description = request.Description,
            IsVerified = false,
            IsPublic = false,
            CreatedAt = _clock.UtcNow
        };
        await _unitOfWork.Repository<OpportunityOutcome>().AddAsync(outcome, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        await _notifications.NotifyAsync(idea.CreatorUserId, NotificationType.OutcomeUpdate,
            "Outcome reported", $"An outcome was reported for \"{idea.Title}\".", null, ct: ct);

        return Result<int>.Ok(outcome.Id);
    }

    public async Task<Result> UpdateAsync(int id, OutcomeUpdateRequest request, CancellationToken ct = default)
    {
        var userId = _current.RequireUserId();
        var outcome = await _unitOfWork.Repository<OpportunityOutcome>().Query().FirstOrDefaultAsync(o => o.Id == id, ct);
        if (outcome is null)
        {
            throw new NotFoundException("Outcome not found.");
        }

        var isReporter = outcome.ReporterUserId == userId;
        if (!isReporter && !_current.IsAdmin)
        {
            throw new ForbiddenException();
        }

        if (outcome.IsVerified && !_current.IsAdmin)
        {
            return Result.Fail("A verified outcome can no longer be edited.");
        }

        outcome.Kind = request.Kind;
        outcome.Status = request.Status;
        outcome.Description = request.Description;
        outcome.UpdatedAt = _clock.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Ok();
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
    {
        var userId = _current.RequireUserId();
        var outcome = await _unitOfWork.Repository<OpportunityOutcome>().Query().FirstOrDefaultAsync(o => o.Id == id, ct);
        if (outcome is null)
        {
            throw new NotFoundException("Outcome not found.");
        }

        var isReporter = outcome.ReporterUserId == userId;
        if (!isReporter && !_current.IsAdmin)
        {
            throw new ForbiddenException();
        }

        if (outcome.IsVerified && !_current.IsAdmin)
        {
            return Result.Fail("A verified outcome can only be removed by an administrator.");
        }

        var ideaId = outcome.IdeaId;
        _unitOfWork.Repository<OpportunityOutcome>().Remove(outcome);
        await _unitOfWork.SaveChangesAsync(ct);

        await _audit.RecordAsync("OutcomeDeleted", nameof(OpportunityOutcome), id.ToString(),
            null, new { ideaId }, ct);
        return Result.Ok();
    }

    public async Task<Result> VerifyAsync(int id, OutcomeVerifyRequest request, CancellationToken ct = default)
    {
        if (!_current.IsAdmin)
        {
            throw new ForbiddenException();
        }

        var outcome = await _unitOfWork.Repository<OpportunityOutcome>().Query().FirstOrDefaultAsync(o => o.Id == id, ct);
        if (outcome is null)
        {
            throw new NotFoundException("Outcome not found.");
        }

        outcome.IsVerified = true;
        outcome.VerifiedByUserId = _current.UserId;
        outcome.VerifiedAt = _clock.UtcNow;
        outcome.IsPublic = request.IsPublic;
        outcome.UpdatedAt = _clock.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        await _audit.RecordAsync("OutcomeVerified", nameof(OpportunityOutcome), outcome.Id.ToString(),
            null, new { request.IsPublic }, ct);
        return Result.Ok();
    }

    private async Task<bool> IsParticipantAsync(int ideaId, int userId, CancellationToken ct)
    {
        var idea = await _unitOfWork.Repository<Idea>().Query().FirstOrDefaultAsync(i => i.Id == ideaId, ct);
        if (idea is not null && idea.CreatorUserId == userId)
        {
            return true;
        }

        return await _unitOfWork.Repository<EntrepreneurInquiry>().Query().AnyAsync(i => i.IdeaId == ideaId && i.SeekerUserId == userId, ct) ||
               await _unitOfWork.Repository<ProjectMember>().Query().AnyAsync(m => m.Project.IdeaId == ideaId && m.UserId == userId, ct);
    }
}


