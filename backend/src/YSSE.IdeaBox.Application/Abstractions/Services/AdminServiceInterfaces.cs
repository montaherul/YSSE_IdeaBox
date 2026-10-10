using YSSE.IdeaBox.Application.Common;
using YSSE.IdeaBox.Application.Dtos;

namespace YSSE.IdeaBox.Application.Abstractions.Services;

public interface IAdminService
{
    Task<PagedResult<AdminUserDto>> GetUsersAsync(AdminUserQuery query, CancellationToken ct = default);

    Task<Result> SuspendUserAsync(int userId, SuspendUserRequest request, CancellationToken ct = default);

    Task<Result> ReactivateUserAsync(int userId, CancellationToken ct = default);

    Task<Result> AssignRoleAsync(int userId, AssignRoleRequest request, CancellationToken ct = default);

    Task<Result> RevokeRoleAsync(int userId, string role, CancellationToken ct = default);

    Task<PagedResult<AuditLogDto>> GetAuditLogsAsync(int page, int pageSize, CancellationToken ct = default);

    Task<IReadOnlyList<PlatformSettingDto>> GetSettingsAsync(CancellationToken ct = default);

    Task<Result> UpdateSettingAsync(PlatformSettingUpdateRequest request, CancellationToken ct = default);

    Task<AdminDashboardDto> GetDashboardAsync(CancellationToken ct = default);
}
