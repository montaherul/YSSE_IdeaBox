using YSSE.IdeaBox.Application.Abstractions;
using YSSE.IdeaBox.Domain.Enums;

namespace YSSE.IdeaBox.Tests;

/// <summary>Deterministic caller derived from the JWT; tests set exactly the members they need.</summary>
internal sealed class FakeCurrentUser : ICurrentUserService
{
    public int? UserId { get; init; }

    public bool IsAuthenticated => UserId is not null;

    public IReadOnlyCollection<string> Roles { get; init; } = Array.Empty<string>();

    public bool IsInRole(string role) => Roles.Contains(role);

    public bool IsModeratorOrAdmin => IsAdmin || Roles.Contains(SystemRoles.Moderator);

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

/// <summary>In-memory file storage double that records saves and supports read/delete round-trips.</summary>
internal sealed class FakeFileStorage : IFileStorage
{
    private readonly Dictionary<string, byte[]> _files = new(StringComparer.OrdinalIgnoreCase);
    private int _counter;

    public Task<string> SaveAsync(Stream content, string fileName, string contentType, CancellationToken ct = default)
    {
        using var buffer = new MemoryStream();
        content.CopyTo(buffer);
        var path = $"file-{++_counter}{Path.GetExtension(fileName)}";
        _files[path] = buffer.ToArray();
        return Task.FromResult(path);
    }

    public Task<Stream?> OpenReadAsync(string storagePath, CancellationToken ct = default)
    {
        Stream? stream = _files.TryGetValue(storagePath, out var bytes)
            ? new MemoryStream(bytes)
            : null;
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string storagePath, CancellationToken ct = default)
    {
        _files.Remove(storagePath);
        return Task.CompletedTask;
    }
}

/// <summary>Permission set double; tests grant exactly the codes they need.</summary>
internal sealed class FakeUserPermissionService : IUserPermissionService
{
    public IReadOnlySet<string> Permissions { get; init; } = new HashSet<string>();

    public Task<IReadOnlySet<string>> GetPermissionsAsync(int userId, CancellationToken ct = default) =>
        Task.FromResult(Permissions);
}
