using System.Security.Cryptography;
using System.Text;
using YSSE.IdeaBox.Application.Abstractions;

namespace YSSE.IdeaBox.Infrastructure.Services;

/// <summary>
/// Issues opaque, high-entropy guest identifiers. Only the SHA-256 hash is ever
/// persisted; the raw value is held client-side by the browser.
/// </summary>
public sealed class GuestIdentityService : IGuestIdentityService
{
    public GuestIdentity Create()
    {
        var bytes = RandomNumberGenerator.GetBytes(16);
        var raw = Convert.ToHexString(bytes).ToLowerInvariant();
        return new GuestIdentity(raw, Hash(raw));
    }

    public string Hash(string rawGuestId)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawGuestId ?? string.Empty));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
