using YSSE.IdeaBox.Domain.Entities;
using YSSE.IdeaBox.Domain.Enums;
using YSSE.IdeaBox.Infrastructure.Services;

namespace YSSE.IdeaBox.Tests;

/// <summary>Effective permissions are aggregated from the user's roles via RBAC tables.</summary>
public class UserPermissionServiceTests
{
    [Fact]
    public async Task Aggregates_distinct_permissions_across_user_roles()
    {
        var (context, _) = TestDb.Create();
        var now = DateTime.UtcNow;

        var user = new User { DisplayName = "Tester", CreatedAt = now };
        context.Users.Add(user);

        var read = new Permission { Code = "users.read", CreatedAt = now };
        var manage = new Permission { Code = "users.manage_roles", CreatedAt = now };
        context.Permissions.AddRange(read, manage);

        var roleA = new Role { Name = SystemRoles.Administrator, Description = "A", CreatedAt = now };
        var roleB = new Role { Name = SystemRoles.Moderator, Description = "B", CreatedAt = now };
        context.Roles.AddRange(roleA, roleB);
        await context.SaveChangesAsync();

        context.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = roleA.Id, GrantedAt = now, CreatedAt = now });
        context.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = roleB.Id, GrantedAt = now, CreatedAt = now });
        context.RolePermissions.Add(new RolePermission { RoleId = roleA.Id, PermissionId = read.Id, CreatedAt = now });
        context.RolePermissions.Add(new RolePermission { RoleId = roleB.Id, PermissionId = manage.Id, CreatedAt = now });
        await context.SaveChangesAsync();

        var permissions = await new UserPermissionService(context).GetPermissionsAsync(user.Id);

        Assert.Equal(2, permissions.Count);
        Assert.Contains("users.read", permissions);
        Assert.Contains("users.manage_roles", permissions);
    }

    [Fact]
    public async Task Returns_empty_for_user_without_roles()
    {
        var (context, _) = TestDb.Create();

        var permissions = await new UserPermissionService(context).GetPermissionsAsync(987654);

        Assert.Empty(permissions);
    }
}
