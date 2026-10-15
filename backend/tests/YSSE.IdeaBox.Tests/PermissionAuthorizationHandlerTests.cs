using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using YSSE.IdeaBox.Api.Security;
using YSSE.IdeaBox.Domain.Entities;
using YSSE.IdeaBox.Domain.Enums;
using YSSE.IdeaBox.Infrastructure.Services;

namespace YSSE.IdeaBox.Tests;

/// <summary>Permission authorization handler grants access only when the user has the required permission.</summary>
public class PermissionAuthorizationHandlerTests
{
    [Fact]
    public async Task Succeeds_when_user_has_permission()
    {
        var (context, _) = TestDb.Create();
        var now = DateTime.UtcNow;

        var user = new User { DisplayName = "Tester", CreatedAt = now };
        context.Users.Add(user);
        var perm = new Permission { Code = "users.read", CreatedAt = now };
        context.Permissions.Add(perm);
        var role = new Role { Name = SystemRoles.Administrator, Description = "A", CreatedAt = now };
        context.Roles.Add(role);
        await context.SaveChangesAsync();
        context.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id, GrantedAt = now, CreatedAt = now });
        context.RolePermissions.Add(new RolePermission { RoleId = role.Id, PermissionId = perm.Id, CreatedAt = now });
        await context.SaveChangesAsync();

        var current = new FakeCurrentUser { UserId = user.Id };
        var handler = new PermissionAuthorizationHandler(current, new UserPermissionService(context), new HttpContextAccessor());
        var requirement = new PermissionRequirement("users.read");
        var contextAuth = new AuthorizationHandlerContext(new[] { requirement },
            new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim("sub", user.Id.ToString())
            }, "test")), null);

        await handler.HandleAsync(contextAuth);

        Assert.True(contextAuth.HasSucceeded);
    }

    [Fact]
    public async Task Fails_when_user_lacks_permission()
    {
        var (context, _) = TestDb.Create();
        var now = DateTime.UtcNow;

        var user = new User { DisplayName = "Tester", CreatedAt = now };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var current = new FakeCurrentUser { UserId = user.Id };
        var handler = new PermissionAuthorizationHandler(current, new UserPermissionService(context), new HttpContextAccessor());
        var requirement = new PermissionRequirement("users.read");
        var contextAuth = new AuthorizationHandlerContext(new[] { requirement },
            new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim("sub", user.Id.ToString())
            }, "test")), null);

        await handler.HandleAsync(contextAuth);

        Assert.False(contextAuth.HasSucceeded);
    }

    [Fact]
    public async Task Requires_authenticated_user()
    {
        var (context, _) = TestDb.Create();
        var current = new FakeCurrentUser { UserId = null };
        var handler = new PermissionAuthorizationHandler(current, new UserPermissionService(context), new HttpContextAccessor());
        var requirement = new PermissionRequirement("users.read");
        var contextAuth = new AuthorizationHandlerContext(new[] { requirement },
            new ClaimsPrincipal(new ClaimsIdentity()), null);

        await handler.HandleAsync(contextAuth);

        Assert.False(contextAuth.HasSucceeded);
    }
}
