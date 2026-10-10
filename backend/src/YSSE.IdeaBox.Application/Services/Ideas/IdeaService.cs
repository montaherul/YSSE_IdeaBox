using Microsoft.EntityFrameworkCore;
using YSSE.IdeaBox.Application.Abstractions;
using YSSE.IdeaBox.Application.Abstractions.Services;
using YSSE.IdeaBox.Application.Common;
using YSSE.IdeaBox.Application.Dtos;
using YSSE.IdeaBox.Application.Services.Common;
using YSSE.IdeaBox.Domain.Entities;
using YSSE.IdeaBox.Domain.Enums;

namespace YSSE.IdeaBox.Application.Services.Ideas;

public sealed class IdeaService : IIdeaService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _current;
    private readonly IDateTimeProvider _clock;
    private readonly IAuditWriter _audit;
    private readonly INotificationDispatcher _notifications;

    public IdeaService(
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

    public async Task<PagedResult<IdeaListItemDto>> SearchAsync(IdeaListQuery query, CancellationToken ct = default)
    {
        var (page, pageSize) = Pagination.Normalize(query.Page, query.PageSize);
        var q = ApplyDiscoveryFilters(_unitOfWork.Repository<Idea>().Query().Where(i => !i.IsDeleted), query, privileged: false);

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderBy(x => x.PublishedAt ?? x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(IdeaProjections.List)
            .ToListAsync(ct);

        return PagedResult<IdeaListItemDto>.Create(items, page, pageSize, total);
    }

    public async Task<IdeaDetailDto?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        var meta = await _unitOfWork.Repository<Idea>().Query()
            .Where(i => i.Slug == slug && !i.IsDeleted)
            .Select(i => new { i.Id, i.Status, i.CreatorUserId })
            .FirstOrDefaultAsync(ct);

        if (meta is null || !CanView(meta.Status, meta.CreatorUserId))
        {
            throw new NotFoundException("Idea not found.");
        }

        var detail = await _unitOfWork.Repository<Idea>().Query()
            .Where(i => i.Id == meta.Id)
            .Select(IdeaProjections.Detail(_current.UserId))
            .FirstOrDefaultAsync(ct);

        if (detail is null)
        {
            throw new NotFoundException("Idea not found.");
        }

        await RecordViewAsync(meta.Id, ct);
        return detail;
    }

    public async Task<IdeaDetailDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var meta = await _unitOfWork.Repository<Idea>().Query()
            .Where(i => i.Id == id && !i.IsDeleted)
            .Select(i => new { i.Id, i.Status, i.CreatorUserId })
            .FirstOrDefaultAsync(ct);

        if (meta is null || !CanView(meta.Status, meta.CreatorUserId))
        {
            throw new NotFoundException("Idea not found.");
        }

        return await _unitOfWork.Repository<Idea>().Query()
            .Where(i => i.Id == meta.Id)
            .Select(IdeaProjections.Detail(_current.UserId))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<Result<int>> CreateAsync(IdeaCreateRequest request, CancellationToken ct = default)
    {
        var userId = _current.RequireUserId();

        var category = await _unitOfWork.Repository<Category>().Query().FirstOrDefaultAsync(c => c.Id == request.CategoryId && c.IsActive, ct);
        if (category is null)
        {
            throw new ValidationAppException(new Dictionary<string, string[]> { ["CategoryId"] = new[] { "Select a valid, active category." } });
        }

        var slug = await UniqueSlugAsync(Slug.From(request.Title, "idea"), ct);

        var idea = new Idea
        {
            Slug = slug,
            Title = request.Title.Trim(),
            ShortSummary = request.ShortSummary?.Trim() ?? string.Empty,
            ProblemStatement = request.ProblemStatement,
            ProposedSolution = request.ProposedSolution,
            TargetAudience = request.TargetAudience,
            ExpectedImpact = request.ExpectedImpact,
            Stage = request.Stage,
            Attribution = request.Attribution,
            CreatorUserId = userId,
            CategoryId = category.Id,
            SupportSought = request.SupportSought,
            SkillsNeeded = request.SkillsNeeded,
            EstimatedBudget = request.EstimatedBudget,
            ExpectedTimeline = request.ExpectedTimeline,
            SuccessMetrics = request.SuccessMetrics,
            CollaborationPreferences = request.CollaborationPreferences,
            LanguageTag = string.IsNullOrWhiteSpace(request.LanguageTag) ? "en" : request.LanguageTag,
            Status = IdeaStatus.Draft,
            CreatedAt = _clock.UtcNow
        };

        await _unitOfWork.Repository<Idea>().AddAsync(idea, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        await RecordStatusChangeAsync(idea, IdeaStatus.Draft, null, ct);
        await ReplaceTagsAsync(idea, request.Tags, ct);

        await _unitOfWork.SaveChangesAsync(ct);
        return Result<int>.Ok(idea.Id);
    }

    public async Task<Result<int>> CreateQuickAsync(QuickIdeaRequest request, CancellationToken ct = default)
    {
        var userId = _current.RequireUserId();

        Category? category = null;
        if (request.CategoryId is not null)
        {
            category = await _unitOfWork.Repository<Category>().Query().FirstOrDefaultAsync(c => c.Id == request.CategoryId && c.IsActive, ct);
        }

        category ??= await _unitOfWork.Repository<Category>().Query().Where(c => c.IsActive).OrderBy(c => c.Id).FirstOrDefaultAsync(ct);
        if (category is null)
        {
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                ["CategoryId"] = new[] { "A valid category is required to create an idea." }
            });
        }

        var title = request.Title.Trim();
        var idea = new Idea
        {
            Slug = await UniqueSlugAsync(Slug.From(title, "idea"), ct),
            Title = title,
            ShortSummary = title.Length <= 300 ? title : title[..300],
            ProblemStatement = request.ProblemStatement,
            ProposedSolution = request.ProposedSolution,
            Stage = DevelopmentStage.Concept,
            Attribution = request.Attribution,
            CreatorUserId = userId,
            CategoryId = category.Id,
            LanguageTag = string.IsNullOrWhiteSpace(request.LanguageTag) ? "en" : request.LanguageTag,
            Status = IdeaStatus.Draft,
            CreatedAt = _clock.UtcNow
        };

        await _unitOfWork.Repository<Idea>().AddAsync(idea, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        await RecordStatusChangeAsync(idea, IdeaStatus.Draft, null, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result<int>.Ok(idea.Id);
    }

    public async Task<Result> UpdateAsync(int id, IdeaUpdateRequest request, CancellationToken ct = default)
    {
        var userId = _current.RequireUserId();
        var idea = await _unitOfWork.Repository<Idea>().Query()
            .Include(i => i.IdeaTags)
            .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted, ct);
        if (idea is null)
        {
            throw new NotFoundException("Idea not found.");
        }

        if (idea.CreatorUserId != userId)
        {
            throw new ForbiddenException("Only the creator can edit this idea.");
        }

        if (idea.Status is not (IdeaStatus.Draft or IdeaStatus.RevisionRequired or IdeaStatus.Published))
        {
            return Result.Fail("Ideas in this status cannot be edited.");
        }

        var category = await _unitOfWork.Repository<Category>().Query().FirstOrDefaultAsync(c => c.Id == request.CategoryId && c.IsActive, ct);
        if (category is null)
        {
            throw new ValidationAppException(new Dictionary<string, string[]> { ["CategoryId"] = new[] { "Select a valid, active category." } });
        }

        await RecordRevisionSnapshotAsync(idea, userId, request.ChangeNote, ct);

        if (idea.Status == IdeaStatus.Published)
        {
            await RecordStatusChangeAsync(idea, IdeaStatus.UnderReview, "Edited after publication; re-entering screening.", ct);
            idea.SubmittedAt = _clock.UtcNow;
        }

        ApplyContent(idea, request, category.Id);
        await ReplaceTagsAsync(idea, request.Tags, ct);

        idea.UpdatedAt = _clock.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Ok();
    }

    public async Task<Result> SubmitAsync(int id, CancellationToken ct = default)
    {
        var userId = _current.RequireUserId();
        var idea = await GetOwnedIdeaAsync(id, userId, ct);
        if (idea is null)
        {
            throw new NotFoundException("Idea not found.");
        }

        if (!IdeaStatusMachine.CanTransition(idea.Status, IdeaStatus.Submitted))
        {
            return Result.Fail($"Ideas cannot transition from {idea.Status} to Submitted.");
        }

        idea.SubmittedAt = _clock.UtcNow;
        await RecordStatusChangeAsync(idea, IdeaStatus.Submitted, null, ct);
        idea.UpdatedAt = _clock.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Ok();
    }

    public async Task<Result> WithdrawAsync(int id, CancellationToken ct = default)
    {
        var userId = _current.RequireUserId();
        var idea = await GetOwnedIdeaAsync(id, userId, ct);
        if (idea is null)
        {
            throw new NotFoundException("Idea not found.");
        }

        if (!IdeaStatusMachine.CanTransition(idea.Status, IdeaStatus.Withdrawn))
        {
            return Result.Fail($"Ideas cannot transition from {idea.Status} to Withdrawn.");
        }

        await RecordStatusChangeAsync(idea, IdeaStatus.Withdrawn, null, ct);
        idea.UpdatedAt = _clock.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Ok();
    }

    public async Task<Result> DeleteDraftAsync(int id, CancellationToken ct = default)
    {
        var userId = _current.RequireUserId();
        var idea = await GetOwnedIdeaAsync(id, userId, ct);
        if (idea is null)
        {
            throw new NotFoundException("Idea not found.");
        }

        if (idea.Status != IdeaStatus.Draft)
        {
            return Result.Fail("Only drafts can be deleted.");
        }

        idea.IsDeleted = true;
        idea.DeletedAt = _clock.UtcNow;
        idea.UpdatedAt = _clock.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        await _audit.RecordAsync("IdeaDraftDeleted", nameof(Idea), idea.Id.ToString(), null, new { idea.Title }, ct);
        return Result.Ok();
    }

    public async Task<PagedResult<IdeaListItemDto>> GetMyIdeasAsync(IdeaListQuery query, CancellationToken ct = default)
    {
        var userId = _current.RequireUserId();
        var (page, pageSize) = Pagination.Normalize(query.Page, query.PageSize);

        var q = _unitOfWork.Repository<Idea>().Query().Where(i => !i.IsDeleted && i.CreatorUserId == userId);
        q = ApplySearchFilters(q, query);

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(IdeaProjections.List)
            .ToListAsync(ct);

        return PagedResult<IdeaListItemDto>.Create(items, page, pageSize, total);
    }

    public async Task<IReadOnlyList<IdeaListItemDto>> GetSimilarAsync(int id, int take = 5, CancellationToken ct = default)
    {
        var idea = await _unitOfWork.Repository<Idea>().Query()
            .Where(i => i.Id == id && !i.IsDeleted)
            .Select(i => new { i.CategoryId })
            .FirstOrDefaultAsync(ct);
        if (idea is null)
        {
            return Array.Empty<IdeaListItemDto>();
        }

        take = Math.Clamp(take, 1, 20);
        return await _unitOfWork.Repository<Idea>().Query()
            .Where(i => i.Id != id && !i.IsDeleted && i.Status == IdeaStatus.Published && i.CategoryId == idea.CategoryId)
            .OrderByDescending(x => x.PublishedAt ?? x.CreatedAt)
            .Take(take)
            .Select(IdeaProjections.List)
            .ToListAsync(ct);
    }

    public async Task RecordViewAsync(int id, CancellationToken ct = default)
    {
        var idea = await _unitOfWork.Repository<Idea>().Query().FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted, ct);
        if (idea is null || !CanView(idea.Status, idea.CreatorUserId))
        {
            return;
        }

        idea.ViewsCount++;
        await _unitOfWork.SaveChangesAsync(ct);
    }

    private bool CanView(IdeaStatus status, int creatorUserId) =>
        status == IdeaStatus.Published ||
        _current.IsModeratorOrAdmin ||
        _current.IsScreeningReviewer ||
        (_current.IsAuthenticated && _current.UserId == creatorUserId);

    private IQueryable<Idea> ApplyDiscoveryFilters(IQueryable<Idea> q, IdeaListQuery query, bool privileged)
    {
        if (privileged && query.Status is not null)
        {
            q = q.Where(x => x.Status == query.Status.Value);
        }
        else
        {
            q = q.Where(x => x.Status == IdeaStatus.Published);
        }

        return ApplySearchFilters(q, query);
    }

    private IQueryable<Idea> ApplySearchFilters(IQueryable<Idea> q, IdeaListQuery query)
    {
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var pattern = $"%{query.Search.Trim().ToLower()}%";
            q = q.Where(x =>
                EF.Functions.Like(x.Title.ToLower(), pattern) ||
                EF.Functions.Like(x.ShortSummary.ToLower(), pattern) ||
                x.Comments.Any(c => !c.IsDeleted && EF.Functions.Like(c.Body.ToLower(), pattern)));
        }

        if (query.CategoryId is not null)
        {
            q = q.Where(x => x.CategoryId == query.CategoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Tag))
        {
            q = q.Where(x => x.IdeaTags.Any(t => t.Tag.Slug == query.Tag || t.Tag.Name == query.Tag));
        }

        if (query.Stage is not null)
        {
            q = q.Where(x => x.Stage == query.Stage.Value);
        }

        q = (query.Sort ?? "newest").ToLowerInvariant() switch
        {
            "oldest" => q.OrderBy(x => x.PublishedAt ?? x.CreatedAt),
            "popular" => q.OrderByDescending(x => x.Votes.Count(v => !v.IsDeleted && v.IsCounted)),
            "most_reviewed" => q.OrderByDescending(x => x.Reviews.Count(r => !r.IsDeleted && r.ModerationStatus == ReviewModerationStatus.Published)),
            _ => q.OrderByDescending(x => x.PublishedAt ?? x.CreatedAt)
        };

        return q;
    }

    private async Task<Idea?> GetOwnedIdeaAsync(int id, int userId, CancellationToken ct) =>
        await _unitOfWork.Repository<Idea>().Query().FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted && i.CreatorUserId == userId, ct);

    private async Task<string> UniqueSlugAsync(string baseSlug, CancellationToken ct)
    {
        var slug = baseSlug;
        var suffix = 2;
        while (await _unitOfWork.Repository<Idea>().Query().AnyAsync(i => i.Slug == slug && !i.IsDeleted, ct))
        {
            slug = $"{baseSlug}-{suffix++}";
        }

        return slug;
    }

    private async Task RecordStatusChangeAsync(Idea idea, IdeaStatus to, string? reason, CancellationToken ct)
    {
        var history = new IdeaStatusHistory
        {
            IdeaId = idea.Id,
            FromStatus = idea.Status,
            ToStatus = to,
            ActorUserId = _current.UserId ?? idea.CreatorUserId,
            Reason = reason,
            CreatedAt = _clock.UtcNow
        };
        idea.Status = to;
        await _unitOfWork.Repository<IdeaStatusHistory>().AddAsync(history, ct);
    }

    private async Task RecordRevisionSnapshotAsync(Idea idea, int userId, string? changeNote, CancellationToken ct)
    {
        var revisionNumber = await _unitOfWork.Repository<IdeaRevision>().Query().CountAsync(r => r.IdeaId == idea.Id, ct) + 1;
        await _unitOfWork.Repository<IdeaRevision>().AddAsync(new IdeaRevision
        {
            IdeaId = idea.Id,
            RevisionNumber = revisionNumber,
            Title = idea.Title,
            ShortSummary = idea.ShortSummary,
            ProblemStatement = idea.ProblemStatement,
            ProposedSolution = idea.ProposedSolution,
            EditedByUserId = userId,
            ChangeNote = changeNote,
            CreatedAt = _clock.UtcNow
        }, ct);
    }

    private async Task ReplaceTagsAsync(Idea idea, IReadOnlyList<string>? tagNames, CancellationToken ct)
    {
        if (tagNames is null || tagNames.Count == 0)
        {
            return;
        }

        var existing = idea.IdeaTags.ToList();
        _unitOfWork.Repository<IdeaTag>().RemoveRange(existing);

        var names = tagNames.Select(t => t.Trim()).Where(t => t.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase).Take(8).ToList();
        foreach (var name in names)
        {
            var tag = await _unitOfWork.Repository<Tag>().Query().FirstOrDefaultAsync(t => t.Name.ToLower() == name.ToLower(), ct);
            if (tag is null)
            {
                tag = new Tag { Name = name, Slug = Slug.From(name, "tag"), CreatedAt = _clock.UtcNow };
                await _unitOfWork.Repository<Tag>().AddAsync(tag, ct);
                await _unitOfWork.SaveChangesAsync(ct);
            }

            await _unitOfWork.Repository<IdeaTag>().AddAsync(new IdeaTag { IdeaId = idea.Id, TagId = tag.Id }, ct);
        }
    }

    private static void ApplyContent(Idea idea, IdeaCreateRequest request, int categoryId)
    {
        idea.Slug = Slug.From(request.Title, "idea");
        idea.Title = request.Title.Trim();
        idea.ShortSummary = request.ShortSummary?.Trim() ?? string.Empty;
        idea.ProblemStatement = request.ProblemStatement;
        idea.ProposedSolution = request.ProposedSolution;
        idea.TargetAudience = request.TargetAudience;
        idea.ExpectedImpact = request.ExpectedImpact;
        idea.Stage = request.Stage;
        idea.Attribution = request.Attribution;
        idea.CategoryId = categoryId;
        idea.SupportSought = request.SupportSought;
        idea.SkillsNeeded = request.SkillsNeeded;
        idea.EstimatedBudget = request.EstimatedBudget;
        idea.ExpectedTimeline = request.ExpectedTimeline;
        idea.SuccessMetrics = request.SuccessMetrics;
        idea.CollaborationPreferences = request.CollaborationPreferences;
        idea.LanguageTag = string.IsNullOrWhiteSpace(request.LanguageTag) ? idea.LanguageTag : request.LanguageTag;
    }
}


