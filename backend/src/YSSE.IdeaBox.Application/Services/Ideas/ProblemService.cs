using Microsoft.EntityFrameworkCore;
using YSSE.IdeaBox.Application.Abstractions;
using YSSE.IdeaBox.Application.Abstractions.Services;
using YSSE.IdeaBox.Application.Common;
using YSSE.IdeaBox.Application.Dtos;
using YSSE.IdeaBox.Domain.Entities;
using YSSE.IdeaBox.Domain.Enums;

using YSSE.IdeaBox.Application.Services.Common;

namespace YSSE.IdeaBox.Application.Services.Ideas;

public sealed class ProblemService : IProblemService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _current;
    private readonly IDateTimeProvider _clock;
    private readonly IAuditWriter _audit;

    public ProblemService(IUnitOfWork db, ICurrentUserService current, IDateTimeProvider clock, IAuditWriter audit)
    {
        _unitOfWork = db;
        _current = current;
        _clock = clock;
        _audit = audit;
    }

    public async Task<PagedResult<ProblemPostDto>> ListAsync(ProblemListQuery query, CancellationToken ct = default)
    {
        var (page, pageSize) = Pagination.Normalize(query.Page, query.PageSize);
        var q = _unitOfWork.Repository<ProblemPost>().Query().Where(p => !p.IsDeleted);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var pattern = $"%{query.Search.Trim().ToLower()}%";
            q = q.Where(p => EF.Functions.Like(p.Title.ToLower(), pattern) ||
                             EF.Functions.Like(p.WhoAffected.ToLower(), pattern) ||
                             EF.Functions.Like(p.WhyItMatters.ToLower(), pattern));
        }

        if (query.Status is not null)
        {
            q = q.Where(p => p.Status == query.Status.Value);
        }

        var userId = _current.UserId;
        var total = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
.Select(p => new ProblemPostDto
            {
                Id = p.Id,
                AuthorUserId = p.AuthorUserId,
                Title = p.Title,
                WhoAffected = p.WhoAffected,
                CoarseLocation = p.CoarseLocation,
                WhyItMatters = p.WhyItMatters,
                Urgency = p.Urgency,
                PhotoUrl = p.PhotoUrl,
                Status = p.Status,
                EndorsementCount = p.Endorsements.Count,
                EndorsedByMe = userId != null && p.Endorsements.Any(e => e.UserId == userId),
                LinkedIdeaCount = p.IdeaLinks.Count,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync(ct);

        return PagedResult<ProblemPostDto>.Create(items, page, pageSize, total);
    }

    public async Task<ProblemPostDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var userId = _current.UserId;
        return await _unitOfWork.Repository<ProblemPost>().Query()
            .Where(p => p.Id == id && !p.IsDeleted)
.Select(p => new ProblemPostDto
            {
                Id = p.Id,
                AuthorUserId = p.AuthorUserId,
                Title = p.Title,
                WhoAffected = p.WhoAffected,
                CoarseLocation = p.CoarseLocation,
                WhyItMatters = p.WhyItMatters,
                Urgency = p.Urgency,
                PhotoUrl = p.PhotoUrl,
                Status = p.Status,
                EndorsementCount = p.Endorsements.Count,
                EndorsedByMe = userId != null && p.Endorsements.Any(e => e.UserId == userId),
                LinkedIdeaCount = p.IdeaLinks.Count,
                CreatedAt = p.CreatedAt
            })
            .FirstOrDefaultAsync(ct);
    }

public async Task<Result<int>> CreateAsync(ProblemPostCreateRequest request, CancellationToken ct = default)
    {
        var userId = _current.RequireUserId();
        var problem = new ProblemPost
        {
            AuthorUserId = userId,
            Title = request.Title.Trim(),
            WhoAffected = request.WhoAffected.Trim(),
            CoarseLocation = request.CoarseLocation,
            WhyItMatters = request.WhyItMatters.Trim(),
            Urgency = request.Urgency,
            PhotoUrl = request.PhotoUrl,
            Status = ProblemStatus.Open,
            CreatedAt = _clock.UtcNow
        };
        await _unitOfWork.Repository<ProblemPost>().AddAsync(problem, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result<int>.Ok(problem.Id);
    }

    public async Task<Result> UpdateAsync(int id, ProblemPostCreateRequest request, CancellationToken ct = default)
    {
        var problem = await _unitOfWork.Repository<ProblemPost>().Query().FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, ct);
        if (problem is null)
        {
            throw new NotFoundException("Problem not found.");
        }

        if (!_current.IsModeratorOrAdmin && problem.AuthorUserId != _current.UserId)
        {
            throw new ForbiddenException();
        }

        problem.Title = request.Title.Trim();
        problem.WhoAffected = request.WhoAffected.Trim();
        problem.CoarseLocation = request.CoarseLocation;
        problem.WhyItMatters = request.WhyItMatters.Trim();
        problem.Urgency = request.Urgency;
        problem.PhotoUrl = request.PhotoUrl;
        problem.UpdatedAt = _clock.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        if (_current.IsAuthenticated)
        {
            await _audit.RecordAsync("ProblemUpdated", nameof(ProblemPost), problem.Id.ToString(),
                null, new { problem.Title }, ct);
        }

        return Result.Ok();
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
    {
        var problem = await _unitOfWork.Repository<ProblemPost>().Query().FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, ct);
        if (problem is null)
        {
            throw new NotFoundException("Problem not found.");
        }

        if (!_current.IsModeratorOrAdmin && problem.AuthorUserId != _current.UserId)
        {
            throw new ForbiddenException();
        }

        problem.IsDeleted = true;
        problem.DeletedAt = _clock.UtcNow;
        problem.UpdatedAt = _clock.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        if (_current.IsAuthenticated)
        {
            await _audit.RecordAsync("ProblemDeleted", nameof(ProblemPost), problem.Id.ToString(),
                null, new { problem.Title }, ct);
        }

        return Result.Ok();
    }

    public async Task<Result> EndorseAsync(int id, CancellationToken ct = default)
    {
        var userId = _current.RequireUserId();
        var problem = await _unitOfWork.Repository<ProblemPost>().Query().FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, ct);
        if (problem is null)
        {
            throw new NotFoundException("Problem not found.");
        }

        var existing = await _unitOfWork.Repository<ProblemEndorsement>().Query().FirstOrDefaultAsync(e => e.ProblemPostId == id && e.UserId == userId, ct);
        if (existing is not null)
        {
            return Result.Fail("You already endorsed this problem.");
        }

        await _unitOfWork.Repository<ProblemEndorsement>().AddAsync(new ProblemEndorsement
        {
            ProblemPostId = id,
            UserId = userId,
            CreatedAt = _clock.UtcNow
        }, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Ok();
    }

    public async Task<Result> ChangeStatusAsync(int id, ProblemStatusChangeRequest request, CancellationToken ct = default)
    {
        var problem = await _unitOfWork.Repository<ProblemPost>().Query().FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, ct);
        if (problem is null)
        {
            throw new NotFoundException("Problem not found.");
        }

        var isAuthor = _current.IsAuthenticated && _current.UserId == problem.AuthorUserId;
        if (!isAuthor && !_current.IsModeratorOrAdmin)
        {
            throw new ForbiddenException();
        }

        problem.Status = request.Status;
        problem.UpdatedAt = _clock.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        if (isAuthor || _current.IsAdmin)
        {
            await _audit.RecordAsync("ProblemStatusChanged", nameof(ProblemPost), problem.Id.ToString(),
                request.Reason, new { request.Status }, ct);
        }

        return Result.Ok();
    }

    public async Task<Result> LinkIdeaAsync(int id, LinkIdeaToProblemRequest request, CancellationToken ct = default)
    {
        var userId = _current.RequireUserId();

        var problem = await _unitOfWork.Repository<ProblemPost>().Query().FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, ct);
        if (problem is null)
        {
            throw new NotFoundException("Problem not found.");
        }

        var idea = await _unitOfWork.Repository<Idea>().Query().FirstOrDefaultAsync(i => i.Id == request.IdeaId && !i.IsDeleted, ct);
        if (idea is null)
        {
            throw new NotFoundException("Idea not found.");
        }

        var isIdeaOwner = idea.CreatorUserId == userId;
        var isProblemAuthor = problem.AuthorUserId == userId;
        if (!isIdeaOwner && !isProblemAuthor && !_current.IsModeratorOrAdmin)
        {
            throw new ForbiddenException("Only the idea creator, problem author or a moderator can link them.");
        }

        var exists = await _unitOfWork.Repository<IdeaProblemLink>().Query().AnyAsync(l => l.IdeaId == request.IdeaId && l.ProblemPostId == id, ct);
        if (exists)
        {
            return Result.Fail("This idea is already linked to the problem.");
        }

        await _unitOfWork.Repository<IdeaProblemLink>().AddAsync(new IdeaProblemLink
        {
            IdeaId = request.IdeaId,
            ProblemPostId = id,
            CreatedAt = _clock.UtcNow
        }, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Ok();
    }
}


