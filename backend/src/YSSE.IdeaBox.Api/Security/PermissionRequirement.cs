using Microsoft.AspNetCore.Authorization;

namespace YSSE.IdeaBox.Api.Security;

/// <summary>
/// Authorization requirement that a caller holds a specific permission code.
/// Policy names are created dynamically from the <c>Permissions</c> catalog.
/// </summary>
public sealed class PermissionRequirement : IAuthorizationRequirement
{
    public PermissionRequirement(string permission) => Permission = permission;

    public string Permission { get; }
}
