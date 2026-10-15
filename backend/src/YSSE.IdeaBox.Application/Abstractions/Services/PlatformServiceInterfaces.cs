using YSSE.IdeaBox.Application.Common;
using YSSE.IdeaBox.Application.Dtos;

namespace YSSE.IdeaBox.Application.Abstractions.Services;

/// <summary>Read access to the append-only audit trail.</summary>
public interface IAuditLogService
{
    Task<PagedResult<AuditLogDto>> GetAuditLogsAsync(int page, int pageSize, CancellationToken ct = default);
}

/// <summary>Platform configuration (key/value settings).</summary>
public interface IPlatformSettingsService
{
    Task<IReadOnlyList<PlatformSettingDto>> GetSettingsAsync(CancellationToken ct = default);

    /// <summary>Applies all supplied keys in a single atomic save.</summary>
    Task<Result> UpdateSettingsAsync(IReadOnlyDictionary<string, string> values, CancellationToken ct = default);
}

/// <summary>Site content blocks (help/about/legal) with a public read surface.</summary>
public interface IStaticContentService
{
    Task<PagedResult<StaticContentDto>> GetStaticContentsAsync(int page, int pageSize, string? search, CancellationToken ct = default);

    Task<Result<int>> CreateStaticContentAsync(StaticContentUpsertRequest request, CancellationToken ct = default);

    Task<Result> UpdateStaticContentAsync(int id, StaticContentUpsertRequest request, CancellationToken ct = default);

    Task<Result> DeleteStaticContentAsync(int id, CancellationToken ct = default);

    Task<StaticContentDto?> GetStaticContentByKeyAsync(string key, CancellationToken ct = default);
}

/// <summary>Aggregated counts and metrics for the administrative dashboard.</summary>
public interface IAdminDashboardService
{
    Task<AdminDashboardDto> GetDashboardAsync(CancellationToken ct = default);
}
