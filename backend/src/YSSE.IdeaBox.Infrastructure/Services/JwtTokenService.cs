using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using YSSE.IdeaBox.Application.Abstractions;

namespace YSSE.IdeaBox.Infrastructure.Services;

/// <summary>
/// HS256 JWT issuer/validator. Access tokens carry the caller's identity and roles;
/// refresh tokens are longer-lived JWTs marked with a <c>typ=refresh</c> claim so they
/// can never be replayed as access tokens.
/// </summary>
public sealed class JwtTokenService : ITokenService
{
    private const string RefreshTypeClaim = "typ";
    private const string RefreshTypeValue = "refresh";
    private const string RoleClaim = "role";

    private readonly string _key;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _accessTokenMinutes;
    private readonly int _refreshTokenDays;
    private readonly IDateTimeProvider _clock;

    public JwtTokenService(IConfiguration configuration, IDateTimeProvider clock)
    {
        var key = configuration["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException(
                "Jwt:Key is not configured. The token service cannot issue or validate tokens without a signing key.");
        }

        _key = key;

        _issuer = configuration["Jwt:Issuer"] ?? "YSSE.IdeaBox";
        _audience = configuration["Jwt:Audience"] ?? "YSSE.IdeaBox";
        _accessTokenMinutes = int.TryParse(configuration["Jwt:AccessTokenMinutes"], out var minutes) && minutes > 0
            ? minutes
            : 60;
        _refreshTokenDays = int.TryParse(configuration["Jwt:RefreshTokenDays"], out var days) && days > 0
            ? days
            : 14;
        _clock = clock;
    }

    public TokenResult CreateTokens(UserTokenInfo user)
    {
        var now = _clock.UtcNow;
        var accessExpiresAt = now.AddMinutes(_accessTokenMinutes);
        var refreshExpiresAt = now.AddDays(_refreshTokenDays);

        var accessClaims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.UserId.ToString(CultureInfo.InvariantCulture)),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new("name", user.DisplayName)
        };

        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            accessClaims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email!));
        }

        foreach (var role in user.Roles)
        {
            accessClaims.Add(new Claim(RoleClaim, role));
        }

        var refreshClaims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.UserId.ToString(CultureInfo.InvariantCulture)),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new(RefreshTypeClaim, RefreshTypeValue)
        };

        return new TokenResult(
            WriteToken(accessClaims, now, accessExpiresAt),
            accessExpiresAt,
            WriteToken(refreshClaims, now, refreshExpiresAt),
            refreshExpiresAt);
    }

    public int? ValidateRefreshToken(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return null;
        }

        var handler = new JwtSecurityTokenHandler { MapInboundClaims = false };

        try
        {
            var principal = handler.ValidateToken(refreshToken, BuildValidationParameters(), out _);

            if (!string.Equals(principal.FindFirst(RefreshTypeClaim)?.Value, RefreshTypeValue, StringComparison.Ordinal))
            {
                return null;
            }

            var subject = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            return int.TryParse(subject, NumberStyles.Integer, CultureInfo.InvariantCulture, out var userId)
                ? userId
                : null;
        }
        catch (SecurityTokenException)
        {
            return null;
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    private string WriteToken(IEnumerable<Claim> claims, DateTime notBefore, DateTime expires)
    {
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(_issuer, _audience, claims, notBefore, expires, credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private TokenValidationParameters BuildValidationParameters() => new()
    {
        ValidateIssuer = true,
        ValidIssuer = _issuer,
        ValidateAudience = true,
        ValidAudience = _audience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key)),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(1)
    };
}
