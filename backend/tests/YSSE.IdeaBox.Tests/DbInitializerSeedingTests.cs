using YSSE.IdeaBox.Application.Common;
using YSSE.IdeaBox.Domain.Enums;
using YSSE.IdeaBox.Infrastructure.Data;

namespace YSSE.IdeaBox.Tests;

/// <summary>
/// Verifies the role -> permission seeding map that makes policy-protected
/// self-service endpoints usable for regular members.
/// </summary>
public class DbInitializerSeedingTests
{
    [Fact]
    public void Member_receives_baseline_self_service_permissions()
    {
        Assert.True(DbInitializer.DefaultRolePermissions.TryGetValue(SystemRoles.Member, out var member));

        Assert.Contains(Permissions.IdeasCreate, member!);
        Assert.Contains(Permissions.ProfilesUpdate, member!);
        Assert.Contains(Permissions.NotificationsRead, member!);
        Assert.Contains(Permissions.MarketplaceManage, member!);
        Assert.DoesNotContain(Permissions.ProjectsCreate, member!);
    }

    [Fact]
    public void Moderator_receives_moderation_permissions()
    {
        Assert.True(DbInitializer.DefaultRolePermissions.TryGetValue(SystemRoles.Moderator, out var moderator));

        Assert.Contains(Permissions.ReviewsModerate, moderator!);
        Assert.Contains(Permissions.ContentReportsManage, moderator!);
        Assert.Contains(Permissions.ScreeningRead, moderator!);
        Assert.Contains(Permissions.ScreeningDecide, moderator!);
    }

    [Fact]
    public void ScreeningReviewer_receives_screening_permissions()
    {
        Assert.True(DbInitializer.DefaultRolePermissions.TryGetValue(SystemRoles.ScreeningReviewer, out var reviewer));

        Assert.Contains(Permissions.ScreeningDecide, reviewer!);
    }

    [Fact]
    public void Administrator_receives_all_permissions()
    {
        Assert.True(DbInitializer.DefaultRolePermissions.TryGetValue(SystemRoles.Administrator, out var admin));

        Assert.Equal(Permissions.All.Length, admin!.Length);
        Assert.All(Permissions.All, code => Assert.Contains(code, admin));
    }

    [Fact]
    public void Every_mapped_permission_is_a_defined_permission()
    {
        var mapped = DbInitializer.DefaultRolePermissions.Values.SelectMany(codes => codes);

        Assert.All(mapped, code => Assert.True(Permissions.IsDefined(code), $"'{code}' is not a defined permission."));
    }
}
