using System.Text.Json;
using YSSE.IdeaBox.Application.Abstractions;
using YSSE.IdeaBox.Domain.Entities;

namespace YSSE.IdeaBox.Infrastructure.Services;

/// <summary>
/// Append-only audit writer (FR-K06, NFR-09). Only metadata is stored; secrets,
/// credentials, and message content are never written.
/// </summary>
public sealed class AuditWriter : IAuditWriter
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _current;
    private readonly IHashingService _hashing;
    private readonly IDateTimeProvider _clock;

    public AuditWriter(IAppDbContext db, ICurrentUserService current, IHashingService hashing, IDateTimeProvider clock)
    {
        _db = db;
        _current = current;
        _hashing = hashing;
        _clock = clock;
    }

    public async Task RecordAsync(string action, string entityName, string? entityId = null,
        string? justification = null, object? metadata = null, CancellationToken ct = default)
    {
        var ipAddress = _current.IpAddress;

        await _db.AuditLogs.AddAsync(new AuditLog
        {
            ActorUserId = _current.UserId,
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            Justification = justification,
            Metadata = metadata is null ? null : JsonSerializer.Serialize(metadata, SerializerOptions),
            IpHash = string.IsNullOrWhiteSpace(ipAddress) ? null : _hashing.Sha256(ipAddress),
            CreatedAt = _clock.UtcNow
        }, ct);

        await _db.SaveChangesAsync(ct);
    }
}
