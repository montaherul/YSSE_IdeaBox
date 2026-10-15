using Microsoft.EntityFrameworkCore;
using YSSE.IdeaBox.Application.Abstractions;
using YSSE.IdeaBox.Application.Abstractions.Services;
using YSSE.IdeaBox.Application.Common;
using YSSE.IdeaBox.Application.Dtos;
using YSSE.IdeaBox.Application.Services.Common;
using YSSE.IdeaBox.Domain.Entities;

namespace YSSE.IdeaBox.Application.Services.Identity;

public sealed class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _current;
    private readonly IDateTimeProvider _clock;
    private readonly IAuditWriter _audit;

    public CategoryService(IUnitOfWork db, ICurrentUserService current, IDateTimeProvider clock, IAuditWriter audit)
    {
        _unitOfWork = db;
        _current = current;
        _clock = clock;
        _audit = audit;
    }

    public async Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(bool includeInactive, CancellationToken ct = default)
    {
        IQueryable<Category> query = _unitOfWork.Repository<Category>().Query();
        if (!includeInactive || !_current.IsAdmin)
        {
            query = query.Where(c => c.IsActive);
        }

        return await query
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                Description = c.Description,
                IsActive = c.IsActive
            })
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<LookupDto>> GetTagsAsync(CancellationToken ct = default) =>
        await _unitOfWork.Repository<Tag>().Query()
            .OrderBy(t => t.Name)
            .Select(t => new LookupDto(t.Id, t.Name))
            .ToListAsync(ct);

    public async Task<PagedResult<TagDto>> GetTagsPagedAsync(int page, int pageSize, string? search, CancellationToken ct = default)
    {
        var (p, s) = Pagination.Normalize(page, pageSize);
        var q = _unitOfWork.Repository<Tag>().Query();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim().ToLower()}%";
            q = q.Where(t => EF.Functions.Like(t.Name.ToLower(), pattern) || EF.Functions.Like(t.Slug.ToLower(), pattern));
        }

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderBy(t => t.Name)
            .Skip((p - 1) * s)
            .Take(s)
            .Select(t => new TagDto
            {
                Id = t.Id,
                Name = t.Name,
                Slug = t.Slug,
                UsageCount = t.IdeaTags.Count,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync(ct);

        return PagedResult<TagDto>.Create(items, p, s, total);
    }

    public async Task<Result<int>> CreateCategoryAsync(CategoryUpsertRequest request, CancellationToken ct = default)
    {
        if (!_current.IsAdmin)
        {
            throw new ForbiddenException();
        }

        var slug = ResolveSlug(request);
        if (await _unitOfWork.Repository<Category>().Query().AnyAsync(c => c.Slug.ToLower() == slug.ToLower(), ct))
        {
            throw new ConflictException("A category with this slug already exists.");
        }

        if (await _unitOfWork.Repository<Category>().Query().AnyAsync(c => c.Name.ToLower() == request.Name.Trim().ToLower(), ct))
        {
            throw new ConflictException("A category with this name already exists.");
        }

        var category = new Category
        {
            Name = request.Name.Trim(),
            Slug = slug,
            Description = request.Description,
            IsActive = request.IsActive,
            CreatedAt = _clock.UtcNow
        };
        await _unitOfWork.Repository<Category>().AddAsync(category, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        await _audit.RecordAsync("CategoryCreated", nameof(Category), category.Id.ToString(), null, new { category.Name }, ct);
        return Result<int>.Ok(category.Id);
    }

    public async Task<Result> UpdateCategoryAsync(int id, CategoryUpsertRequest request, CancellationToken ct = default)
    {
        if (!_current.IsAdmin)
        {
            throw new ForbiddenException();
        }

        var category = await _unitOfWork.Repository<Category>().Query().FirstOrDefaultAsync(c => c.Id == id, ct);
        if (category is null)
        {
            throw new NotFoundException("Category not found.");
        }

        var slug = ResolveSlug(request);
        if (await _unitOfWork.Repository<Category>().Query().AnyAsync(c => c.Slug.ToLower() == slug.ToLower() && c.Id != id, ct))
        {
            throw new ConflictException("A category with this slug already exists.");
        }

        category.Name = request.Name.Trim();
        category.Slug = slug;
        category.Description = request.Description;
        category.IsActive = request.IsActive;
        category.UpdatedAt = _clock.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        await _audit.RecordAsync("CategoryUpdated", nameof(Category), category.Id.ToString(), null, new { category.Name }, ct);
        return Result.Ok();
    }

    public async Task<Result> DeleteCategoryAsync(int id, CancellationToken ct = default)
    {
        if (!_current.IsAdmin)
        {
            throw new ForbiddenException();
        }

        var category = await _unitOfWork.Repository<Category>().Query().FirstOrDefaultAsync(c => c.Id == id, ct);
        if (category is null)
        {
            throw new NotFoundException("Category not found.");
        }

        category.IsActive = false;
        category.UpdatedAt = _clock.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        await _audit.RecordAsync("CategoryDeleted", nameof(Category), category.Id.ToString(), null, new { category.Name }, ct);
        return Result.Ok();
    }

    public async Task<Result<int>> CreateTagAsync(TagUpsertRequest request, CancellationToken ct = default)
    {
        if (!_current.IsAdmin)
        {
            throw new ForbiddenException();
        }

        var slug = string.IsNullOrWhiteSpace(request.Slug) ? Slug.From(request.Name, "tag") : Slug.From(request.Slug!, "tag");
        if (await _unitOfWork.Repository<Tag>().Query().AnyAsync(t => t.Slug.ToLower() == slug.ToLower(), ct))
        {
            throw new ConflictException("A tag with this slug already exists.");
        }

        if (await _unitOfWork.Repository<Tag>().Query().AnyAsync(t => t.Name.ToLower() == request.Name.Trim().ToLower(), ct))
        {
            throw new ConflictException("A tag with this name already exists.");
        }

        var tag = new Tag
        {
            Name = request.Name.Trim(),
            Slug = slug,
            CreatedAt = _clock.UtcNow
        };
        await _unitOfWork.Repository<Tag>().AddAsync(tag, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        await _audit.RecordAsync("TagCreated", nameof(Tag), tag.Id.ToString(), null, new { tag.Name }, ct);
        return Result<int>.Ok(tag.Id);
    }

    public async Task<Result> UpdateTagAsync(int id, TagUpsertRequest request, CancellationToken ct = default)
    {
        if (!_current.IsAdmin)
        {
            throw new ForbiddenException();
        }

        var tag = await _unitOfWork.Repository<Tag>().Query().FirstOrDefaultAsync(t => t.Id == id, ct);
        if (tag is null)
        {
            throw new NotFoundException("Tag not found.");
        }

        var slug = string.IsNullOrWhiteSpace(request.Slug) ? Slug.From(request.Name, "tag") : Slug.From(request.Slug!, "tag");
        if (await _unitOfWork.Repository<Tag>().Query().AnyAsync(t => t.Slug.ToLower() == slug.ToLower() && t.Id != id, ct))
        {
            throw new ConflictException("A tag with this slug already exists.");
        }

        tag.Name = request.Name.Trim();
        tag.Slug = slug;
        await _unitOfWork.SaveChangesAsync(ct);

        await _audit.RecordAsync("TagUpdated", nameof(Tag), tag.Id.ToString(), null, new { tag.Name }, ct);
        return Result.Ok();
    }

    public async Task<Result> DeleteTagAsync(int id, CancellationToken ct = default)
    {
        if (!_current.IsAdmin)
        {
            throw new ForbiddenException();
        }

        var tag = await _unitOfWork.Repository<Tag>().Query().FirstOrDefaultAsync(t => t.Id == id, ct);
        if (tag is null)
        {
            throw new NotFoundException("Tag not found.");
        }

        var inUse = await _unitOfWork.Repository<IdeaTag>().Query().AnyAsync(it => it.TagId == id, ct);
        if (inUse)
        {
            return Result.Fail("Cannot delete tag that is in use by ideas.");
        }

        _unitOfWork.Repository<Tag>().Remove(tag);
        await _unitOfWork.SaveChangesAsync(ct);

        await _audit.RecordAsync("TagDeleted", nameof(Tag), tag.Id.ToString(), null, new { tag.Name }, ct);
        return Result.Ok();
    }

    private static string ResolveSlug(CategoryUpsertRequest request) =>
        string.IsNullOrWhiteSpace(request.Slug) ? Slug.From(request.Name, "category") : Slug.From(request.Slug, "category");
}


