using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using YSSE.IdeaBox.Application.Common;

namespace YSSE.IdeaBox.Api.Security;

/// <summary>
/// Supplies authorization policies on demand. Registered policies (Admin, Moderator, …)
/// are resolved by the default provider; any name that matches a known permission code
/// becomes a permission policy backed by <see cref="PermissionRequirement"/>.
/// </summary>
public sealed class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallback;

    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options) =>
        _fallback = new DefaultAuthorizationPolicyProvider(options);

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _fallback.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => _fallback.GetFallbackPolicyAsync();

    public async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        var registered = await _fallback.GetPolicyAsync(policyName);
        if (registered is not null)
        {
            return registered;
        }

        if (Permissions.IsDefined(policyName))
        {
            return new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new PermissionRequirement(policyName))
                .Build();
        }

        return null;
    }
}
