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
    private IReadOnlyCollection<RoleName>? _roles;

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

    public IReadOnlyCollection<RoleName> Roles
    {
        get
        {
            if (_roles is not null)
            {
                return _roles;
            }

            var roles = new List<RoleName>();
            if (Principal is not null)
            {
                var claims = Principal.FindAll(RoleClaim)
                    .Concat(Principal.FindAll(ClaimTypes.Role));

                foreach (var claim in claims)
                {
                    if (Enum.TryParse<RoleName>(claim.Value, ignoreCase: true, out var role) && !roles.Contains(role))
                    {
                        roles.Add(role);
                    }
                }
            }

            return _roles = roles;
        }
    }

    public bool IsInRole(RoleName role) => Roles.Contains(role);

    public bool IsModeratorOrAdmin => IsInRole(RoleName.Moderator) || IsInRole(RoleName.Administrator);

    public bool IsAdmin => IsInRole(RoleName.Administrator);

    public bool IsScreeningReviewer => IsInRole(RoleName.ScreeningReviewer) || IsInRole(RoleName.Administrator);

    public string? IpAddress => _accessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
}
