using Microsoft.Extensions.Diagnostics.HealthChecks;
using YSSE.IdeaBox.Application.Abstractions;

namespace YSSE.IdeaBox.Api.Health;

/// <summary>
/// Readiness probe that confirms the API can reach PostgreSQL.
/// </summary>
public sealed class DatabaseHealthCheck : IHealthCheck
{
    private readonly ISqlQueryExecutor _sql;

    public DatabaseHealthCheck(ISqlQueryExecutor sql) => _sql = sql;

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _sql.QuerySingleAsync<int>("SELECT 1", ct: cancellationToken);
            return result == 1
                ? HealthCheckResult.Healthy("Database reachable.")
                : HealthCheckResult.Unhealthy("Database returned an unexpected result.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database unreachable.", ex);
        }
    }
}
