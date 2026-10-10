using Microsoft.EntityFrameworkCore;
using YSSE.IdeaBox.Application.Abstractions;
using YSSE.IdeaBox.Application.Abstractions.Services;
using YSSE.IdeaBox.Application.Common;
using YSSE.IdeaBox.Application.Dtos;
using YSSE.IdeaBox.Domain.Entities;
using YSSE.IdeaBox.Domain.Enums;

namespace YSSE.IdeaBox.Application.Services.Engagement;

public sealed class ReportService : IReportService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _current;
    private readonly IDateTimeProvider _clock;
    private readonly IAuditWriter _audit;

    public ReportService(IUnitOfWork db, ICurrentUserService current, IDateTimeProvider clock, IAuditWriter audit)
    {
        _unitOfWork = db;
        _current = current;
        _clock = clock;
        _audit = audit;
    }

    public async Task<PagedResult<ReportDto>> ListAsync(ReportStatus? status, int page, int pageSize, CancellationToken ct = default)
    {
        if (!_current.IsModeratorOrAdmin)
        {
            throw new ForbiddenException();
        }

        var (p, s) = Pagination.Normalize(page, pageSize);
        var q = _unitOfWork.Repository<Report>().Query().AsQueryable();
        if (status is not null)
        {
            q = q.Where(r => r.Status == status.Value);
        }

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(r => r.CreatedAt)
            .Skip((p - 1) * s)
            .Take(s)
            .Select(r => new
            {
                r.Id,
                r.TargetType,
                r.TargetId,
                r.Reason,
                r.Details,
                r.Status,
                r.ReporterUserId,
                r.CreatedAt,
                r.ResolvedAt,
                r.Resolution
            })
            .ToListAsync(ct);

        var reporterIds = items.Where(x => x.ReporterUserId != 0).Select(x => x.ReporterUserId).Distinct().ToList();
        var reporterNames = await _unitOfWork.Repository<User>().Query()
            .Where(u => reporterIds.Contains(u.Id))
            .Select(u => new { u.Id, u.DisplayName })
            .ToDictionaryAsync(u => u.Id, u => u.DisplayName, ct);

        var result = items.Select(x => new ReportDto
        {
            Id = x.Id,
            TargetType = x.TargetType,
            TargetId = x.TargetId,
            Reason = x.Reason,
            Details = x.Details,
            Status = x.Status,
            ReporterName = reporterNames.TryGetValue(x.ReporterUserId, out var name) ? name : "System",
            CreatedAt = x.CreatedAt,
            ResolvedAt = x.ResolvedAt,
            Resolution = x.Resolution
        }).ToList();

        return PagedResult<ReportDto>.Create(result, p, s, total);
    }

    public async Task<Result> ResolveAsync(int id, ReportResolveRequest request, CancellationToken ct = default)
    {
        if (!_current.IsModeratorOrAdmin)
        {
            throw new ForbiddenException();
        }

        var report = await _unitOfWork.Repository<Report>().Query().FirstOrDefaultAsync(r => r.Id == id, ct);
        if (report is null)
        {
            throw new NotFoundException("Report not found.");
        }

        report.Status = request.Status;
        report.Resolution = request.Resolution;
        report.HandledByUserId = _current.UserId;
        report.ResolvedAt = _clock.UtcNow;
        report.UpdatedAt = _clock.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        await _audit.RecordAsync("ReportResolved", nameof(Report), report.Id.ToString(),
            request.Resolution, new { request.Status }, ct);
        return Result.Ok();
    }
}


