using System.Collections.Concurrent;
using YSSE.IdeaBox.Application.Abstractions;

namespace YSSE.IdeaBox.Infrastructure.Services;

/// <summary>
/// In-process verification code store for the demo (email/SMS OTP). Registered as a
/// singleton; swappable for a distributed cache without changing consumers.
/// </summary>
public sealed class InMemoryVerificationCodeStore : IVerificationCodeStore
{
    private readonly ConcurrentDictionary<string, Entry> _codes = new();

    public void Store(int userId, string purpose, string code, TimeSpan lifetime)
        => _codes[BuildKey(userId, purpose)] = new Entry(code, DateTime.UtcNow.Add(lifetime));

    public bool TryConsume(int userId, string purpose, string code)
    {
        var key = BuildKey(userId, purpose);

        if (!_codes.TryGetValue(key, out var entry))
        {
            return false;
        }

        if (entry.ExpiresAt <= DateTime.UtcNow)
        {
            _codes.TryRemove(key, out _);
            return false;
        }

        if (!string.Equals(entry.Code, code, StringComparison.Ordinal))
        {
            return false;
        }

        _codes.TryRemove(key, out _);
        return true;
    }

    private static string BuildKey(int userId, string purpose) => $"{userId}:{purpose}";

    private sealed record Entry(string Code, DateTime ExpiresAt);
}
