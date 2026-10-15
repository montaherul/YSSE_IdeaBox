using Microsoft.EntityFrameworkCore;
using YSSE.IdeaBox.Application.Abstractions;
using YSSE.IdeaBox.Application.Abstractions.Services;
using YSSE.IdeaBox.Application.Common;
using YSSE.IdeaBox.Application.Dtos;
using YSSE.IdeaBox.Domain.Entities;

namespace YSSE.IdeaBox.Application.Services.Platform;

public sealed class StaticContentService : IStaticContentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _clock;
    private readonly IAuditWriter _audit;

    public StaticContentService(IUnitOfWork unitOfWork, IDateTimeProvider clock, IAuditWriter audit)
    {
        _unitOfWork = unitOfWork;
        _clock = clock;
        _audit = audit;
    }

    public async Task<PagedResult<StaticContentDto>> GetStaticContentsAsync(int page, int pageSize, string? search, CancellationToken ct = default)
    {
        var (p, s) = Pagination.Normalize(page, pageSize);
        var q = _unitOfWork.Repository<StaticContent>().Query().Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim().ToLower()}%";
            q = q.Where(x => EF.Functions.Like(x.Key.ToLower(), pattern) ||
                             EF.Functions.Like(x.Title.ToLower(), pattern));
        }

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderBy(x => x.Key)
            .Skip((p - 1) * s)
            .Take(s)
            .Select(x => new StaticContentDto
            {
                Id = x.Id,
                Key = x.Key,
                Title = x.Title,
                Content = x.Content,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync(ct);

        return PagedResult<StaticContentDto>.Create(items, p, s, total);
    }

    public async Task<Result<int>> CreateStaticContentAsync(StaticContentUpsertRequest request, CancellationToken ct = default)
    {
        if (await _unitOfWork.Repository<StaticContent>().Query().AnyAsync(x => x.Key == request.Key, ct))
        {
            throw new ConflictException("A static content with this key already exists.");
        }

        var content = new StaticContent
        {
            Key = request.Key.Trim(),
            Title = request.Title.Trim(),
            Content = request.Content,
            IsActive = request.IsActive,
            CreatedAt = _clock.UtcNow
        };
        await _unitOfWork.Repository<StaticContent>().AddAsync(content, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        await _audit.RecordAsync("StaticContentCreated", nameof(StaticContent), content.Id.ToString(), null, new { content.Key }, ct);
        return Result<int>.Ok(content.Id);
    }

    public async Task<Result> UpdateStaticContentAsync(int id, StaticContentUpsertRequest request, CancellationToken ct = default)
    {
        var content = await _unitOfWork.Repository<StaticContent>().Query().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (content is null)
        {
            throw new NotFoundException("Static content not found.");
        }

        if (await _unitOfWork.Repository<StaticContent>().Query().AnyAsync(x => x.Key == request.Key && x.Id != id, ct))
        {
            throw new ConflictException("A static content with this key already exists.");
        }

        content.Key = request.Key.Trim();
        content.Title = request.Title.Trim();
        content.Content = request.Content;
        content.IsActive = request.IsActive;
        content.UpdatedAt = _clock.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        await _audit.RecordAsync("StaticContentUpdated", nameof(StaticContent), content.Id.ToString(), null, new { content.Key }, ct);
        return Result.Ok();
    }

    public async Task<Result> DeleteStaticContentAsync(int id, CancellationToken ct = default)
    {
        var content = await _unitOfWork.Repository<StaticContent>().Query().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (content is null)
        {
            throw new NotFoundException("Static content not found.");
        }

        content.IsDeleted = true;
        content.DeletedAt = _clock.UtcNow;
        content.UpdatedAt = _clock.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        await _audit.RecordAsync("StaticContentDeleted", nameof(StaticContent), content.Id.ToString(), null, new { content.Key }, ct);
        return Result.Ok();
    }

    public async Task<StaticContentDto?> GetStaticContentByKeyAsync(string key, CancellationToken ct = default)
    {
        var content = await _unitOfWork.Repository<StaticContent>().Query()
            .FirstOrDefaultAsync(x => x.Key == key && !x.IsDeleted && x.IsActive, ct);
        if (content is null)
        {
            return null;
        }

        return new StaticContentDto
        {
            Id = content.Id,
            Key = content.Key,
            Title = content.Title,
            Content = content.Content,
            IsActive = content.IsActive,
            CreatedAt = content.CreatedAt,
            UpdatedAt = content.UpdatedAt
        };
    }
}
