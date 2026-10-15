using YSSE.IdeaBox.Application.Abstractions;
using YSSE.IdeaBox.Application.Dtos;

namespace YSSE.IdeaBox.Infrastructure.Queries;

/// <summary>
/// Dapper-backed read model for the admin dashboard. SQL is PostgreSQL-specific and
/// kept in the Infrastructure layer; all values are passed as parameters.
/// </summary>
public sealed class AdminDashboardQueries : IAdminDashboardQueries
{
    private readonly ISqlQueryExecutor _executor;

    public AdminDashboardQueries(ISqlQueryExecutor executor) => _executor = executor;

    private const string IdeasPerMonthSql = """
        SELECT to_char(date_trunc('month', created_at), 'YYYY-MM') AS "Label",
               count(*)::text AS "Value"
        FROM ideas
        WHERE is_deleted = false
        GROUP BY date_trunc('month', created_at)
        ORDER BY date_trunc('month', created_at) DESC
        LIMIT 12
        """;

    public async Task<IReadOnlyList<DashboardMetricDto>> GetIdeasPerMonthAsync(CancellationToken ct = default)
        => await _executor.QueryAsync<DashboardMetricDto>(IdeasPerMonthSql, null, ct);
}
