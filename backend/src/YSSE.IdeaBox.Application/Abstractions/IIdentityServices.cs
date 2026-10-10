using YSSE.IdeaBox.Domain.Enums;

namespace YSSE.IdeaBox.Application.Abstractions;

/// <summary>Authenticated caller derived from the JWT (server is the source of truth).</summary>
public interface ICurrentUserService
{
    int? UserId { get; }

    bool IsAuthenticated { get; }

    IReadOnlyCollection<RoleName> Roles { get; }

    bool IsInRole(RoleName role);

    bool IsModeratorOrAdmin { get; }

    bool IsAdmin { get; }

    bool IsScreeningReviewer { get; }

    /// <summary>Best-effort client IP for rate limiting / hashed audit data.</summary>
    string? IpAddress { get; }
}

public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string password, string hash);
}

public record TokenResult(string AccessToken, DateTime ExpiresAt, string RefreshToken, DateTime RefreshExpiresAt);

public interface ITokenService
{
    TokenResult CreateTokens(UserTokenInfo user);

    int? ValidateRefreshToken(string refreshToken);
}

public record UserTokenInfo(int UserId, string DisplayName, string? Email, IReadOnlyCollection<RoleName> Roles);

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}

/// <summary>Opaque guest identifier issuance/validation for the best-effort review limit.</summary>
public record GuestIdentity(string RawGuestId, string Hash);

public interface IGuestIdentityService
{
    GuestIdentity Create();

    string Hash(string rawGuestId);
}

/// <summary>
/// Short-lived store for account verification codes (email/SMS OTP).
/// Implemented as an in-process cache for the demo; swappable for Redis/distributed store.
/// </summary>
public interface IVerificationCodeStore
{
    void Store(int userId, string purpose, string code, TimeSpan lifetime);

    bool TryConsume(int userId, string purpose, string code);
}

/// <summary>Hashes short-lived, privacy-sensitive values (guest id, IP, refresh token).</summary>
public interface IHashingService
{
    string Sha256(string value);
}
