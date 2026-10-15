using Microsoft.AspNetCore.Authorization;
using YSSE.IdeaBox.Application.Abstractions;

namespace YSSE.IdeaBox.Api.Security;

/// <summary>
/// Grants a <see cref="PermissionRequirement"/> when the authenticated caller holds the
/// permission through their roles. Permissions are resolved from the database and cached
/// for the lifetime of the request only, so revocations apply on the next request.
/// </summary>
public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private const string CacheKey = "__ideabox_user_permissions";
    private readonly ICurrentUserService _current;
    private readonly IUserPermissionService _permissions;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PermissionAuthorizationHandler(
        ICurrentUserService current,
        IUserPermissionService permissions,
        IHttpContextAccessor httpContextAccessor)
    {
        _current = current;
        _permissions = permissions;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User?.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var userId = _current.UserId;
        if (userId is null)
        {
            return;
        }

        var granted = await ResolveAsync(userId.Value);
        if (granted.Contains(requirement.Permission))
        {
            context.Succeed(requirement);
        }
    }

    private async Task<IReadOnlySet<string>> ResolveAsync(int userId)
    {
        var items = _httpContextAccessor.HttpContext?.Items;
        if (items is not null && items.TryGetValue(CacheKey, out var cached) && cached is IReadOnlySet<string> set)
        {
            return set;
        }

        var resolved = await _permissions.GetPermissionsAsync(userId);
        if (items is not null)
        {
            items[CacheKey] = resolved;
        }

        return resolved;
    }
}
