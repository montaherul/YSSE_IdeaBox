using Microsoft.EntityFrameworkCore;
using YSSE.IdeaBox.Application.Abstractions;
using YSSE.IdeaBox.Application.Common;
using YSSE.IdeaBox.Application.Dtos;
using YSSE.IdeaBox.Application.Services.Ideas;
using YSSE.IdeaBox.Domain.Entities;
using YSSE.IdeaBox.Domain.Enums;

namespace YSSE.IdeaBox.Tests;

/// <summary>
/// Problem post CRUD (Module C) driven through the real GenericRepository/UnitOfWork
/// over the EF InMemory provider. Ownership and moderator rules are enforced server-side.
/// </summary>
public class ProblemServiceTests
{
    private static readonly DateTime Now = new(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc);

    private static ProblemService CreateService(IUnitOfWork uow, FakeCurrentUser current)
        => new(uow, current, new FakeClock(Now), new NoOpAuditWriter());

    private static async Task<ProblemPost> AddProblemAsync(IUnitOfWork uow, int authorId)
    {
        var problem = new ProblemPost
        {
            AuthorUserId = authorId,
            Title = "Original title",
            WhoAffected = "Affected",
            WhyItMatters = "Why it matters",
            Status = ProblemStatus.Open,
            Attribution = "Public",
            CreatedAt = Now
        };
        await uow.Repository<ProblemPost>().AddAsync(problem);
        return problem;
    }

    private static ProblemPostCreateRequest NewRequest() => new()
    {
        Title = "Updated title",
        WhoAffected = "Updated affected",
        CoarseLocation = "Dhaka",
        WhyItMatters = "Updated why it matters",
        Urgency = "High",
        PhotoUrl = null
    };

    [Fact]
    public async Task Update_requires_authenticated_owner()
    {
        var (_, uow) = TestDb.Create();
        var problem = await AddProblemAsync(uow, 1);
        await uow.SaveChangesAsync();

        var svc = CreateService(uow, new FakeCurrentUser()); // unauthenticated
        await Assert.ThrowsAsync<ForbiddenException>(() => svc.UpdateAsync(problem.Id, NewRequest()));

        var other = CreateService(uow, new FakeCurrentUser { UserId = 2 });
        await Assert.ThrowsAsync<ForbiddenException>(() => other.UpdateAsync(problem.Id, NewRequest()));
    }

    [Fact]
    public async Task Update_missing_problem_throws_not_found()
    {
        var (_, uow) = TestDb.Create();
        var svc = CreateService(uow, new FakeCurrentUser { UserId = 1 });

        await Assert.ThrowsAsync<NotFoundException>(() => svc.UpdateAsync(999, NewRequest()));
    }

    [Fact]
    public async Task Update_owner_can_edit_fields()
    {
        var (_, uow) = TestDb.Create();
        var problem = await AddProblemAsync(uow, 1);
        await uow.SaveChangesAsync();

        var svc = CreateService(uow, new FakeCurrentUser { UserId = 1 });
        var result = await svc.UpdateAsync(problem.Id, NewRequest());

        Assert.True(result.Success);
        var stored = await uow.Repository<ProblemPost>().Query().FirstAsync(p => p.Id == problem.Id);
        Assert.Equal("Updated title", stored.Title);
        Assert.Equal("Updated affected", stored.WhoAffected);
        Assert.Equal("Dhaka", stored.CoarseLocation);
        Assert.Equal("High", stored.Urgency);
        Assert.Equal(Now, stored.UpdatedAt);
    }

    [Fact]
    public async Task Update_moderator_can_edit_another_users_problem()
    {
        var (_, uow) = TestDb.Create();
        var problem = await AddProblemAsync(uow, 2);
        await uow.SaveChangesAsync();

        var svc = CreateService(uow, new FakeCurrentUser { UserId = 1, IsAdmin = true });
        var result = await svc.UpdateAsync(problem.Id, NewRequest());

        Assert.True(result.Success);
        var stored = await uow.Repository<ProblemPost>().Query().FirstAsync(p => p.Id == problem.Id);
        Assert.Equal("Updated title", stored.Title);
    }

    [Fact]
    public async Task Delete_requires_authenticated_owner()
    {
        var (_, uow) = TestDb.Create();
        var problem = await AddProblemAsync(uow, 1);
        await uow.SaveChangesAsync();

        var svc = CreateService(uow, new FakeCurrentUser { UserId = 2 });
        await Assert.ThrowsAsync<ForbiddenException>(() => svc.DeleteAsync(problem.Id));
    }

    [Fact]
    public async Task Delete_missing_problem_throws_not_found()
    {
        var (_, uow) = TestDb.Create();
        var svc = CreateService(uow, new FakeCurrentUser { UserId = 1 });

        await Assert.ThrowsAsync<NotFoundException>(() => svc.DeleteAsync(999));
    }

    [Fact]
    public async Task Delete_soft_deletes_for_owner()
    {
        var (_, uow) = TestDb.Create();
        var problem = await AddProblemAsync(uow, 1);
        await uow.SaveChangesAsync();

        var svc = CreateService(uow, new FakeCurrentUser { UserId = 1 });
        var result = await svc.DeleteAsync(problem.Id);

        Assert.True(result.Success);
        var stored = await uow.Repository<ProblemPost>().Query().FirstAsync(p => p.Id == problem.Id);
        Assert.True(stored.IsDeleted);
        Assert.Equal(Now, stored.DeletedAt);
        Assert.Null(await svc.GetByIdAsync(problem.Id));
    }

    [Fact]
    public async Task Update_deleted_problem_throws_not_found()
    {
        var (_, uow) = TestDb.Create();
        var problem = await AddProblemAsync(uow, 1);
        problem.IsDeleted = true;
        problem.DeletedAt = Now;
        await uow.SaveChangesAsync();

        var svc = CreateService(uow, new FakeCurrentUser { UserId = 1 });
        await Assert.ThrowsAsync<NotFoundException>(() => svc.UpdateAsync(problem.Id, NewRequest()));
    }
}