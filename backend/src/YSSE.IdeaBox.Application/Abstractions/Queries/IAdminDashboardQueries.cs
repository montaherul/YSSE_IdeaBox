using YSSE.IdeaBox.Application.Dtos;

namespace YSSE.IdeaBox.Application.Abstractions;

/// <summary>
/// Read-model queries for the admin dashboard. The SQL lives in the Infrastructure
/// implementation so the Application layer stays free of persistence concerns.
/// </summary>
public interface IAdminDashboardQueries
{
    /// <summary>Monthly idea-creation counts (most recent first) for the dashboard chart.</summary>
    Task<IReadOnlyList<DashboardMetricDto>> GetIdeasPerMonthAsync(CancellationToken ct = default);
}
