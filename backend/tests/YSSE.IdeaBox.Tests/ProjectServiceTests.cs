using Microsoft.EntityFrameworkCore;
using YSSE.IdeaBox.Application.Abstractions;
using YSSE.IdeaBox.Application.Common;
using YSSE.IdeaBox.Application.Dtos;
using YSSE.IdeaBox.Application.Services.Engagement;
using YSSE.IdeaBox.Domain.Entities;
using YSSE.IdeaBox.Domain.Enums;

namespace YSSE.IdeaBox.Tests;

/// <summary>
/// Project workspace rules: staff-assigned creation with an entrepreneur/investor lead,
/// participant-only visibility (members never see projects), and the admin
/// archive-as-success flow.
/// </summary>
public class ProjectServiceTests
{
    private static readonly DateTime Now = new(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc);

    private static ProjectService CreateService(IUnitOfWork uow, FakeCurrentUser current)
        => new(uow, current, new FakeClock(Now), new NoOpAuditWriter(), new NoOpNotificationDispatcher());

    private static async Task<Idea> SeedIdeaAsync(IUnitOfWork uow, int creatorId, IdeaStatus status = IdeaStatus.Published)
    {
        var idea = new Idea
        {
            Slug = $"idea-{Guid.NewGuid():N}",
            Title = "Community solar micro-grid",
            ShortSummary = "Summary",
            CategoryId = 1,
            CreatorUserId = creatorId,
            Status = status,
            CreatedAt = Now
        };
        await uow.Repository<Idea>().AddAsync(idea);
        await uow.SaveChangesAsync();
        return idea;
    }

    private static async Task<User> SeedUserAsync(IUnitOfWork uow, string displayName, params string[] roles)
    {
        var user = new User { DisplayName = displayName, CreatedAt = Now };
        await uow.Repository<User>().AddAsync(user);
        await uow.SaveChangesAsync();

        foreach (var roleName in roles)
        {
            var role = new Role { Name = roleName, Description = roleName, CreatedAt = Now };
            await uow.Repository<Role>().AddAsync(role);
            await uow.SaveChangesAsync();
            await uow.Repository<UserRole>().AddAsync(new UserRole
            {
                UserId = user.Id,
                RoleId = role.Id,
                GrantedAt = Now
            });
        }

        await uow.SaveChangesAsync();
        return user;
    }

    private static async Task<Project> SeedProjectAsync(
        IUnitOfWork uow,
        int ideaId,
        int leadUserId,
        ProjectStatus status = ProjectStatus.Planned,
        params int[] memberIds)
    {
        var project = new Project
        {
            IdeaId = ideaId,
            Name = "Solar pilot",
            Status = status,
            LeadUserId = leadUserId,
            CreatedAt = Now,
            CompletedAt = status is ProjectStatus.Completed or ProjectStatus.Archived ? Now : null,
            ArchivedAt = status == ProjectStatus.Archived ? Now : null
        };
        await uow.Repository<Project>().AddAsync(project);
        await uow.SaveChangesAsync();

        await uow.Repository<ProjectMember>().AddAsync(new ProjectMember
        {
            ProjectId = project.Id,
            UserId = leadUserId,
            Role = "Lead",
            CreatedAt = Now
        });
        foreach (var memberId in memberIds)
        {
            await uow.Repository<ProjectMember>().AddAsync(new ProjectMember
            {
                ProjectId = project.Id,
                UserId = memberId,
                Role = "Member",
                CreatedAt = Now
            });
        }

        await uow.SaveChangesAsync();
        return project;
    }

    private static ProjectCreateRequest CreateRequest(int ideaId, int leadUserId) => new()
    {
        IdeaId = ideaId,
        LeadUserId = leadUserId,
        Name = "Solar pilot",
        Objectives = "Deliver a pilot"
    };

    [Fact]
    public async Task Create_requires_staff()
    {
        var (_, uow) = TestDb.Create();
        var idea = await SeedIdeaAsync(uow, creatorId: 5);
        var lead = await SeedUserAsync(uow, "Investor", SystemRoles.Investor);

        var svc = CreateService(uow, new FakeCurrentUser { UserId = 10 });

        await Assert.ThrowsAsync<ForbiddenException>(
            () => svc.CreateAsync(CreateRequest(idea.Id, lead.Id)));
    }

    [Fact]
    public async Task Create_assigns_selected_investor_as_lead_and_member()
    {
        var (_, uow) = TestDb.Create();
        var idea = await SeedIdeaAsync(uow, creatorId: 5);
        var lead = await SeedUserAsync(uow, "Investor", SystemRoles.Investor);

        var svc = CreateService(uow, new FakeCurrentUser { UserId = 2, IsAdmin = true });
        var result = await svc.CreateAsync(CreateRequest(idea.Id, lead.Id));

        Assert.True(result.Success);
        var project = await uow.Repository<Project>().Query().FirstAsync();
        Assert.Equal(lead.Id, project.LeadUserId);
        Assert.Equal(ProjectStatus.Planned, project.Status);

        var member = await uow.Repository<ProjectMember>().Query().FirstAsync(m => m.ProjectId == project.Id);
        Assert.Equal(lead.Id, member.UserId);
        Assert.Equal("Lead", member.Role);
    }

    [Fact]
    public async Task Create_rejects_lead_without_entrepreneur_or_investor_role()
    {
        var (_, uow) = TestDb.Create();
        var idea = await SeedIdeaAsync(uow, creatorId: 5);
        var lead = await SeedUserAsync(uow, "Plain member", SystemRoles.Member);

        var svc = CreateService(uow, new FakeCurrentUser { UserId = 2, IsAdmin = true });
        var result = await svc.CreateAsync(CreateRequest(idea.Id, lead.Id));

        Assert.False(result.Success);
        Assert.Empty(await uow.Repository<Project>().Query().ToListAsync());
    }

    [Fact]
    public async Task Create_rejects_unknown_lead()
    {
        var (_, uow) = TestDb.Create();
        var idea = await SeedIdeaAsync(uow, creatorId: 5);

        var svc = CreateService(uow, new FakeCurrentUser { UserId = 2, IsAdmin = true });
        var result = await svc.CreateAsync(CreateRequest(idea.Id, 999));

        Assert.False(result.Success);
    }

    [Fact]
    public async Task Create_rejects_draft_idea_for_moderator()
    {
        var (_, uow) = TestDb.Create();
        var idea = await SeedIdeaAsync(uow, creatorId: 5, IdeaStatus.Draft);
        var lead = await SeedUserAsync(uow, "Entrepreneur", SystemRoles.Entrepreneur);

        var svc = CreateService(uow, new FakeCurrentUser { UserId = 2, Roles = new[] { SystemRoles.Moderator } });
        var result = await svc.CreateAsync(CreateRequest(idea.Id, lead.Id));

        Assert.False(result.Success);
    }

    [Fact]
    public async Task GetForIdea_hidden_from_regular_members()
    {
        var (_, uow) = TestDb.Create();
        var idea = await SeedIdeaAsync(uow, creatorId: 5);
        await SeedProjectAsync(uow, idea.Id, leadUserId: 6);

        var svc = CreateService(uow, new FakeCurrentUser { UserId = 10 });

        await Assert.ThrowsAsync<ForbiddenException>(() => svc.GetForIdeaAsync(idea.Id));
    }

    [Fact]
    public async Task GetForIdea_visible_to_idea_creator_and_staff()
    {
        var (_, uow) = TestDb.Create();
        var idea = await SeedIdeaAsync(uow, creatorId: 5);
        await SeedProjectAsync(uow, idea.Id, leadUserId: 6);

        var creator = CreateService(uow, new FakeCurrentUser { UserId = 5 });
        Assert.Single(await creator.GetForIdeaAsync(idea.Id));

        var staff = CreateService(uow, new FakeCurrentUser { UserId = 2, IsAdmin = true });
        Assert.Single(await staff.GetForIdeaAsync(idea.Id));
    }

    [Fact]
    public async Task GetById_allowed_for_project_member_but_not_outsiders()
    {
        var (_, uow) = TestDb.Create();
        var idea = await SeedIdeaAsync(uow, creatorId: 5);
        var project = await SeedProjectAsync(uow, idea.Id, leadUserId: 6, ProjectStatus.Planned, 7);

        var member = CreateService(uow, new FakeCurrentUser { UserId = 7 });
        var detail = await member.GetByIdAsync(project.Id);
        Assert.NotNull(detail);
        Assert.Equal(project.Id, detail!.Id);

        var outsider = CreateService(uow, new FakeCurrentUser { UserId = 99 });
        await Assert.ThrowsAsync<ForbiddenException>(() => outsider.GetByIdAsync(project.Id));
    }

    [Fact]
    public async Task Archive_requires_admin()
    {
        var (_, uow) = TestDb.Create();
        var idea = await SeedIdeaAsync(uow, creatorId: 5);
        var project = await SeedProjectAsync(uow, idea.Id, leadUserId: 6, ProjectStatus.Completed);

        var moderator = CreateService(uow, new FakeCurrentUser { UserId = 2, Roles = new[] { SystemRoles.Moderator } });

        await Assert.ThrowsAsync<ForbiddenException>(() => moderator.ArchiveAsync(project.Id));
    }

    [Fact]
    public async Task Archive_requires_completed_status()
    {
        var (_, uow) = TestDb.Create();
        var idea = await SeedIdeaAsync(uow, creatorId: 5);
        var project = await SeedProjectAsync(uow, idea.Id, leadUserId: 6, ProjectStatus.InProgress);

        var admin = CreateService(uow, new FakeCurrentUser { UserId = 2, IsAdmin = true });
        var result = await admin.ArchiveAsync(project.Id);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task Archive_marks_completed_project_as_success()
    {
        var (_, uow) = TestDb.Create();
        var idea = await SeedIdeaAsync(uow, creatorId: 5);
        var project = await SeedProjectAsync(uow, idea.Id, leadUserId: 6, ProjectStatus.Completed);

        var admin = CreateService(uow, new FakeCurrentUser { UserId = 2, IsAdmin = true });
        var result = await admin.ArchiveAsync(project.Id);

        Assert.True(result.Success);
        var stored = await uow.Repository<Project>().Query().FirstAsync(p => p.Id == project.Id);
        Assert.Equal(ProjectStatus.Archived, stored.Status);
        Assert.Equal(Now, stored.ArchivedAt);
    }

    [Fact]
    public async Task ChangeStatus_cannot_archive_directly_and_archived_is_final()
    {
        var (_, uow) = TestDb.Create();
        var idea = await SeedIdeaAsync(uow, creatorId: 5);
        var project = await SeedProjectAsync(uow, idea.Id, leadUserId: 6, ProjectStatus.Completed);

        var admin = CreateService(uow, new FakeCurrentUser { UserId = 2, IsAdmin = true });
        var direct = await admin.ChangeStatusAsync(project.Id, new ProjectStatusChangeRequest(ProjectStatus.Archived));
        Assert.False(direct.Success);

        Assert.True((await admin.ArchiveAsync(project.Id)).Success);
        var afterArchive = await admin.ChangeStatusAsync(project.Id, new ProjectStatusChangeRequest(ProjectStatus.InProgress));
        Assert.False(afterArchive.Success);
    }

    [Fact]
    public async Task GetArchived_lists_successes_for_admin_only()
    {
        var (_, uow) = TestDb.Create();
        var idea = await SeedIdeaAsync(uow, creatorId: 5);
        var lead = await SeedUserAsync(uow, "Lead Investor", SystemRoles.Investor);
        await SeedProjectAsync(uow, idea.Id, lead.Id, ProjectStatus.Archived);
        await SeedProjectAsync(uow, idea.Id, lead.Id, ProjectStatus.InProgress);

        var admin = CreateService(uow, new FakeCurrentUser { UserId = 2, IsAdmin = true });
        var page = await admin.GetArchivedAsync(new ProjectArchiveQuery());

        var item = Assert.Single(page.Items);
        Assert.Equal(lead.DisplayName, item.LeadDisplayName);
        Assert.Equal(idea.Title, item.IdeaTitle);
        Assert.Equal(Now, item.ArchivedAt);

        var member = CreateService(uow, new FakeCurrentUser { UserId = 10 });
        await Assert.ThrowsAsync<ForbiddenException>(() => member.GetArchivedAsync(new ProjectArchiveQuery()));
    }

    [Fact]
    public async Task GetMine_returns_assigned_projects_for_lead_creator_and_members()
    {
        var (_, uow) = TestDb.Create();
        var idea = await SeedIdeaAsync(uow, creatorId: 5);
        var lead = await SeedUserAsync(uow, "Lead", SystemRoles.Investor);
        await SeedProjectAsync(uow, idea.Id, lead.Id, ProjectStatus.InProgress, 9);

        var leadSvc = CreateService(uow, new FakeCurrentUser { UserId = lead.Id });
        var creatorSvc = CreateService(uow, new FakeCurrentUser { UserId = 5 });
        var memberSvc = CreateService(uow, new FakeCurrentUser { UserId = 9 });
        var outsiderSvc = CreateService(uow, new FakeCurrentUser { UserId = 99 });

        Assert.Single(await leadSvc.GetMineAsync());
        Assert.Single(await creatorSvc.GetMineAsync());
        Assert.Single(await memberSvc.GetMineAsync());
        Assert.Empty(await outsiderSvc.GetMineAsync());
    }

    [Fact]
    public async Task GetLeadCandidates_returns_only_entrepreneurs_and_investors()
    {
        var (_, uow) = TestDb.Create();
        var investor = await SeedUserAsync(uow, "Investor", SystemRoles.Investor);
        var entrepreneur = await SeedUserAsync(uow, "Entrepreneur", SystemRoles.Entrepreneur);
        await SeedUserAsync(uow, "Plain member", SystemRoles.Member);

        var staff = CreateService(uow, new FakeCurrentUser { UserId = 2, IsAdmin = true });
        var candidates = await staff.GetLeadCandidatesAsync(null);

        Assert.Equal(2, candidates.Count);
        Assert.Contains(candidates, c => c.UserId == investor.Id);
        Assert.Contains(candidates, c => c.UserId == entrepreneur.Id);
    }

    [Fact]
    public async Task GetLeadCandidates_denies_non_staff()
    {
        var (_, uow) = TestDb.Create();
        var svc = CreateService(uow, new FakeCurrentUser { UserId = 10 });

        await Assert.ThrowsAsync<ForbiddenException>(() => svc.GetLeadCandidatesAsync(null));
    }
}
