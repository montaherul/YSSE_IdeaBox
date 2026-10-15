using Microsoft.EntityFrameworkCore;
using YSSE.IdeaBox.Application.Abstractions;
using YSSE.IdeaBox.Application.Abstractions.Services;
using YSSE.IdeaBox.Application.Common;
using YSSE.IdeaBox.Application.Dtos;
using YSSE.IdeaBox.Domain.Entities;

namespace YSSE.IdeaBox.Application.Services.Identity;

public sealed class PermissionService : IPermissionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _clock;
    private readonly IAuditWriter _audit;

    public PermissionService(IUnitOfWork unitOfWork, IDateTimeProvider clock, IAuditWriter audit)
    {
        _unitOfWork = unitOfWork;
        _clock = clock;
        _audit = audit;
    }

    public async Task<IReadOnlyList<PermissionDto>> GetPermissionsAsync(CancellationToken ct = default)
    {
        return await _unitOfWork.Repository<Permission>().Query()
            .OrderBy(p => p.Code)
            .Select(p => new PermissionDto
            {
                Id = p.Id,
                Code = p.Code,
                Description = p.Description,
                RoleCount = p.RolePermissions.Count,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync(ct);
    }

    public async Task<PagedResult<PermissionDto>> GetPermissionsPagedAsync(int page, int pageSize, string? search, CancellationToken ct = default)
    {
        var (p, s) = Pagination.Normalize(page, pageSize);
        var q = _unitOfWork.Repository<Permission>().Query();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim().ToLower()}%";
            q = q.Where(x => EF.Functions.Like(x.Code.ToLower(), pattern) ||
                             EF.Functions.Like(x.Description.ToLower(), pattern));
        }

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderBy(x => x.Code)
            .Skip((p - 1) * s)
            .Take(s)
            .Select(x => new PermissionDto
            {
                Id = x.Id,
                Code = x.Code,
                Description = x.Description,
                RoleCount = x.RolePermissions.Count,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(ct);

        return PagedResult<PermissionDto>.Create(items, p, s, total);
    }

    public async Task<PermissionDto?> GetPermissionByIdAsync(int id, CancellationToken ct = default)
    {
        return await _unitOfWork.Repository<Permission>().Query()
            .Where(p => p.Id == id)
            .Select(p => new PermissionDto
            {
                Id = p.Id,
                Code = p.Code,
                Description = p.Description,
                RoleCount = p.RolePermissions.Count,
                CreatedAt = p.CreatedAt
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<Result<int>> CreatePermissionAsync(PermissionUpsertRequest request, CancellationToken ct = default)
    {
        if (await _unitOfWork.Repository<Permission>().Query().AnyAsync(p => p.Code == request.Code, ct))
        {
            throw new ConflictException("Permission code already exists.");
        }

        var permission = new Permission
        {
            Code = request.Code.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            CreatedAt = _clock.UtcNow
        };
        await _unitOfWork.Repository<Permission>().AddAsync(permission, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        await _audit.RecordAsync("PermissionCreated", nameof(Permission), permission.Id.ToString(), null, new { permission.Code }, ct);
        return Result<int>.Ok(permission.Id);
    }

    public async Task<Result> UpdatePermissionAsync(int id, PermissionUpsertRequest request, CancellationToken ct = default)
    {
        var permission = await _unitOfWork.Repository<Permission>().Query().FirstOrDefaultAsync(p => p.Id == id, ct);
        if (permission is null)
        {
            throw new NotFoundException("Permission not found.");
        }

        if (await _unitOfWork.Repository<Permission>().Query().AnyAsync(p => p.Code == request.Code && p.Id != id, ct))
        {
            throw new ConflictException("Permission code already in use.");
        }

        permission.Code = request.Code.Trim();
        permission.Description = request.Description?.Trim() ?? string.Empty;
        await _unitOfWork.SaveChangesAsync(ct);

        await _audit.RecordAsync("PermissionUpdated", nameof(Permission), permission.Id.ToString(), null, new { permission.Code }, ct);
        return Result.Ok();
    }

    public async Task<Result> DeletePermissionAsync(int id, CancellationToken ct = default)
    {
        var permission = await _unitOfWork.Repository<Permission>().Query().FirstOrDefaultAsync(p => p.Id == id, ct);
        if (permission is null)
        {
            throw new NotFoundException("Permission not found.");
        }

        var inUse = await _unitOfWork.Repository<RolePermission>().Query().AnyAsync(rp => rp.PermissionId == id, ct);
        if (inUse)
        {
            return Result.Fail("Cannot delete permission that is assigned to roles.");
        }

        _unitOfWork.Repository<Permission>().Remove(permission);
        await _unitOfWork.SaveChangesAsync(ct);

        await _audit.RecordAsync("PermissionDeleted", nameof(Permission), permission.Id.ToString(), null, new { permission.Code }, ct);
        return Result.Ok();
    }
}
