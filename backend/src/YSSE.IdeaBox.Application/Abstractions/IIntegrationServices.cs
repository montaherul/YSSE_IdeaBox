namespace YSSE.IdeaBox.Application.Abstractions;

/// <summary>
/// Executes complex, read-only SQL (leaderboards, matching, dashboards) via Dapper.
/// Kept separate from the EF-backed <see cref="IAppDbContext"/> so reporting queries
/// never leak into the write model.
/// </summary>
public interface ISqlQueryExecutor
{
    Task<IReadOnlyList<T>> QueryAsync<T>(string sql, object? parameters = null, CancellationToken ct = default);

    Task<T?> QuerySingleAsync<T>(string sql, object? parameters = null, CancellationToken ct = default);

    Task<int> ExecuteAsync(string sql, object? parameters = null, CancellationToken ct = default);
}

/// <summary>Append-only audit writer (FR-K06, NFR-09). Never logs secrets or message content.</summary>
public interface IAuditWriter
{
    Task RecordAsync(string action, string entityName, string? entityId = null,
        string? justification = null, object? metadata = null, CancellationToken ct = default);
}

/// <summary>Dispatches in-app/email notifications honoring user preferences (Module J).</summary>
public interface INotificationDispatcher
{
    Task NotifyAsync(int userId, Domain.Enums.NotificationType type, string title, string body,
        string? linkUrl = null, bool essential = false, CancellationToken ct = default);
}

/// <summary>Private file storage with short-lived signed links (FR-G04, NFR-02).</summary>
public interface IFileStorage
{
    Task<string> SaveAsync(Stream content, string fileName, string contentType, CancellationToken ct = default);

    Task<Stream?> OpenReadAsync(string storagePath, CancellationToken ct = default);

    Task DeleteAsync(string storagePath, CancellationToken ct = default);
}
