using System.Security.Cryptography;
using System.Text;
using YSSE.IdeaBox.Application.Abstractions;

namespace YSSE.IdeaBox.Infrastructure.Services;

/// <summary>SHA-256 hashing for short-lived, privacy-sensitive values.</summary>
public sealed class HashingService : IHashingService
{
    public string Sha256(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
