using YSSE.IdeaBox.Application.Abstractions;
using YSSE.IdeaBox.Domain.Enums;

namespace YSSE.IdeaBox.Tests;

/// <summary>Deterministic caller derived from the JWT; tests set exactly the members they need.</summary>
internal sealed class FakeCurrentUser : ICurrentUserService
{
    public int? UserId { get; init; }

    public bool IsAuthenticated => UserId is not null;

    public IReadOnlyCollection<RoleName> Roles { get; init; } = Array.Empty<RoleName>();

    public bool IsInRole(RoleName role) => Roles.Contains(role);

    public bool IsModeratorOrAdmin => IsAdmin;

    public bool IsAdmin { get; init; }

    public bool IsScreeningReviewer { get; init; }

    public string? IpAddress => null;
}

/// <summary>Fixed-clock implementation of IDateTimeProvider for deterministic window tests.</summary>
internal sealed class FakeClock : IDateTimeProvider
{
    public FakeClock(DateTime utcNow) => UtcNow = utcNow;

    public DateTime UtcNow { get; set; }
}

/// <summary>No-op audit writer; the Service contract for audit is exercised by the API layer.</summary>
internal sealed class NoOpAuditWriter : IAuditWriter
{
    public Task RecordAsync(string action, string entityName, string? entityId = null,
        string? justification = null, object? metadata = null, CancellationToken ct = default) =>
        Task.CompletedTask;
}

/// <summary>No-op notification dispatcher; not under test in these suites.</summary>
internal sealed class NoOpNotificationDispatcher : INotificationDispatcher
{
    public Task NotifyAsync(int userId, NotificationType type, string title, string body,
        string? linkUrl = null, bool essential = false, CancellationToken ct = default) =>
        Task.CompletedTask;
}

/// <summary>Guards against accidental Dapper/raw-SQL execution inside data-driven unit tests.</summary>
internal sealed class ThrowingSqlQueryExecutor : ISqlQueryExecutor
{
    public Task<IReadOnlyList<T>> QueryAsync<T>(string sql, object? parameters = null, CancellationToken ct = default) =>
        throw new InvalidOperationException("Raw SQL execution is not available in unit tests.");

    public Task<T?> QuerySingleAsync<T>(string sql, object? parameters = null, CancellationToken ct = default) =>
        throw new InvalidOperationException("Raw SQL execution is not available in unit tests.");

    public Task<int> ExecuteAsync(string sql, object? parameters = null, CancellationToken ct = default) =>
        throw new InvalidOperationException("Raw SQL execution is not available in unit tests.");
}