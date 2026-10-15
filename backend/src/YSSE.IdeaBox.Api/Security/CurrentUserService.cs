using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using YSSE.IdeaBox.Application.Abstractions;
using YSSE.IdeaBox.Domain.Enums;

namespace YSSE.IdeaBox.Api.Security;

/// <summary>
/// Resolves the authenticated caller from the JWT carried on <see cref="HttpContext.User"/>.
/// The server is always the source of truth; no client-supplied identity is trusted.
/// </summary>
public sealed class CurrentUserService : ICurrentUserService
{
    private const string RoleClaim = "role";
    private readonly IHttpContextAccessor _accessor;
    private IReadOnlyCollection<string>? _roles;

    public CurrentUserService(IHttpContextAccessor accessor) => _accessor = accessor;

    private ClaimsPrincipal? Principal => _accessor.HttpContext?.User;

    public int? UserId
    {
        get
        {
            var raw = Principal?.FindFirstValue("sub")
                ?? Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id) ? id : null;
        }
    }

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

    public IReadOnlyCollection<string> Roles
    {
        get
        {
            if (_roles is not null)
            {
                return _roles;
            }

            var roles = new List<string>();
            if (Principal is not null)
            {
                var claims = Principal.FindAll(RoleClaim)
                    .Concat(Principal.FindAll(ClaimTypes.Role));

                foreach (var claim in claims)
                {
                    if (!string.IsNullOrWhiteSpace(claim.Value) && !roles.Contains(claim.Value, StringComparer.OrdinalIgnoreCase))
                    {
                        roles.Add(claim.Value);
                    }
                }
            }

            return _roles = roles;
        }
    }

    public bool IsInRole(string role) =>
        !string.IsNullOrWhiteSpace(role) && Roles.Contains(role, StringComparer.OrdinalIgnoreCase);

    public bool IsModeratorOrAdmin => IsInRole(SystemRoles.Moderator) || IsInRole(SystemRoles.Administrator);

    public bool IsAdmin => IsInRole(SystemRoles.Administrator);

    public bool IsScreeningReviewer => IsInRole(SystemRoles.ScreeningReviewer) || IsInRole(SystemRoles.Administrator);

    public string? IpAddress => _accessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
}
