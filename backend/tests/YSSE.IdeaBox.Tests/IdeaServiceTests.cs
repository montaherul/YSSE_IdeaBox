using Microsoft.EntityFrameworkCore;
using YSSE.IdeaBox.Application.Abstractions;
using YSSE.IdeaBox.Application.Dtos;
using YSSE.IdeaBox.Application.Services.Ideas;
using YSSE.IdeaBox.Domain.Entities;
using YSSE.IdeaBox.Domain.Enums;

namespace YSSE.IdeaBox.Tests;

/// <summary>
/// Idea editing must keep the unique slug index satisfied: renaming an idea to a title
/// that slugifies to an existing idea's slug must not produce a duplicate slug.
/// </summary>
public class IdeaServiceTests
{
    private static readonly DateTime Now = new(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc);

    private static IdeaService CreateService(IUnitOfWork uow, FakeCurrentUser current)
        => new(uow, current, new FakeClock(Now), new NoOpAuditWriter(), new NoOpNotificationDispatcher(),
            new FakeFileStorage(), new FakeUserPermissionService());

    private static IdeaUpdateRequest UpdateRequest(string title, int categoryId) => new()
    {
        Title = title,
        ShortSummary = "Summary",
        ProblemStatement = "Problem",
        ProposedSolution = "Solution",
        TargetAudience = "Audience",
        ExpectedImpact = "Impact",
        Stage = DevelopmentStage.Concept,
        Attribution = IdeaAttribution.Public,
        CategoryId = categoryId
    };

    private static async Task<int> SeedCategoryAsync(IUnitOfWork uow)
    {
        var category = new Category { Name = "Test", Slug = "test", IsActive = true, CreatedAt = Now };
        await uow.Repository<Category>().AddAsync(category);
        await uow.SaveChangesAsync();
        return category.Id;
    }

    private static async Task<Idea> SeedIdeaAsync(IUnitOfWork uow, int categoryId, string slug, string title)
    {
        var idea = new Idea
        {
            Slug = slug,
            Title = title,
            ShortSummary = "Summary",
            CategoryId = categoryId,
            CreatorUserId = 99,
            Status = IdeaStatus.Draft,
            CreatedAt = Now
        };
        await uow.Repository<Idea>().AddAsync(idea);
        await uow.SaveChangesAsync();
        return idea;
    }

    [Fact]
    public async Task Update_renaming_to_existing_slug_produces_unique_slug()
    {
        var (_, uow) = TestDb.Create();
        var categoryId = await SeedCategoryAsync(uow);
        await SeedIdeaAsync(uow, categoryId, "alpha-idea", "Alpha Idea");
        var second = await SeedIdeaAsync(uow, categoryId, "beta-idea", "Beta Idea");

        var svc = CreateService(uow, new FakeCurrentUser { UserId = 99 });
        var result = await svc.UpdateAsync(second.Id, UpdateRequest("Alpha Idea", categoryId));

        Assert.True(result.Success);
        var updated = await uow.Repository<Idea>().Query().FirstAsync(i => i.Id == second.Id);
        Assert.Equal("alpha-idea-2", updated.Slug);
    }

    [Fact]
    public async Task Update_without_title_change_keeps_slug_stable()
    {
        var (_, uow) = TestDb.Create();
        var categoryId = await SeedCategoryAsync(uow);
        var idea = await SeedIdeaAsync(uow, categoryId, "alpha-idea", "Alpha Idea");

        var svc = CreateService(uow, new FakeCurrentUser { UserId = 99 });
        var result = await svc.UpdateAsync(idea.Id, UpdateRequest("Alpha Idea", categoryId));

        Assert.True(result.Success);
        var updated = await uow.Repository<Idea>().Query().FirstAsync(i => i.Id == idea.Id);
        Assert.Equal("alpha-idea", updated.Slug);
    }
}
