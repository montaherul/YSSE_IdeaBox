using Microsoft.EntityFrameworkCore;
using YSSE.IdeaBox.Application.Abstractions;
using YSSE.IdeaBox.Infrastructure.Data;

namespace YSSE.IdeaBox.Infrastructure.Services;

/// <summary>
/// Reads a user's effective permissions from the RBAC tables. Permissions are always
/// resolved from the database (never trusted from the client), so role changes and
/// permission revocations take effect on the next request.
/// </summary>
public sealed class UserPermissionService : IUserPermissionService
{
    private readonly AppDbContext _db;

    public UserPermissionService(AppDbContext db) => _db = db;

    public async Task<IReadOnlySet<string>> GetPermissionsAsync(int userId, CancellationToken ct = default)
    {
        var codes = await _db.UserRoles
            .Where(ur => ur.UserId == userId)
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Code)
            .Distinct()
            .ToListAsync(ct);

        return codes.ToHashSet(StringComparer.OrdinalIgnoreCase);
    }
}
