using Microsoft.EntityFrameworkCore;
using YSSE.IdeaBox.Application.Abstractions;
using YSSE.IdeaBox.Application.Common;
using YSSE.IdeaBox.Application.Dtos;
using YSSE.IdeaBox.Application.Services.Engagement;
using YSSE.IdeaBox.Domain.Entities;
using YSSE.IdeaBox.Domain.Enums;

namespace YSSE.IdeaBox.Tests;

/// <summary>
/// Voting eligibility rules (Module E) driven through the real GenericRepository/UnitOfWork
/// over the EF InMemory provider. Window, verification and account-age rules are FR-E06/FR-E07.
/// </summary>
public class VotingServiceTests
{
    private static readonly DateTime Now = new(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc);

    private static VotingService CreateService(IUnitOfWork uow, FakeCurrentUser current)
        => new(uow, current, new FakeClock(Now), new NoOpAuditWriter());

    private static async Task<User> AddUserAsync(IUnitOfWork uow, string displayName,
        bool verified = true, bool suspended = false, DateTime? createdAt = null)
    {
        var user = new User
        {
            DisplayName = displayName,
            IsEmailVerified = verified,
            IsPhoneVerified = verified,
            IsSuspended = suspended,
            CreatedAt = createdAt ?? Now.AddDays(-90)
        };
        await uow.Repository<User>().AddAsync(user);
        return user;
    }

    private static async Task<Idea> AddIdeaAsync(IUnitOfWork uow, int creatorId, IdeaStatus status)
    {
        var idea = new Idea
        {
            Slug = "idea-" + Guid.NewGuid().ToString("N")[..6],
            Title = "Idea",
            ShortSummary = "Summary",
            CategoryId = 1,
            CreatorUserId = creatorId,
            Status = status,
            PublishedAt = status is IdeaStatus.Published or IdeaStatus.Shortlisted ? Now : null
        };
        await uow.Repository<Idea>().AddAsync(idea);
        return idea;
    }

    private static async Task<VotingPeriod> AddPeriodAsync(IUnitOfWork uow,
        bool active = true, bool closed = false, DateTime? startAt = null, DateTime? endAt = null,
        bool requiresVerified = false, int minAgeDays = 0, VoteVisibility visibility = VoteVisibility.HiddenUntilClose)
    {
        var period = new VotingPeriod
        {
            Name = "P1",
            StartAt = startAt ?? Now.AddDays(-1),
            EndAt = endAt ?? Now.AddDays(1),
            RequiresVerifiedAccount = requiresVerified,
            MinimumAccountAgeDays = minAgeDays,
            IsActive = active,
            IsClosed = closed,
            Visibility = visibility
        };
        await uow.Repository<VotingPeriod>().AddAsync(period);
        return period;
    }

    [Fact]
    public async Task Cast_requires_authenticated_user()
    {
        var (_, uow) = TestDb.Create();
        var svc = CreateService(uow, new FakeCurrentUser());

        await Assert.ThrowsAsync<UnauthorizedException>(() => svc.CastAsync(1));
    }

    [Fact]
    public async Task Cast_missing_idea_throws_not_found()
    {
        var (_, uow) = TestDb.Create();
        var voter = await AddUserAsync(uow, "Voter");
        await uow.SaveChangesAsync();
        var svc = CreateService(uow, new FakeCurrentUser { UserId = voter.Id });

        await Assert.ThrowsAsync<NotFoundException>(() => svc.CastAsync(999));
    }

    [Theory]
    [InlineData(IdeaStatus.Draft)]
    [InlineData(IdeaStatus.Submitted)]
    [InlineData(IdeaStatus.UnderReview)]
    [InlineData(IdeaStatus.Rejected)]
    [InlineData(IdeaStatus.Archived)]
    public async Task Cast_rejects_ideas_outside_eligible_statuses(IdeaStatus status)
    {
        var (_, uow) = TestDb.Create();
        var creator = await AddUserAsync(uow, "Creator");
        var voter = await AddUserAsync(uow, "Voter");
        var idea = await AddIdeaAsync(uow, creator.Id, status);
        await AddPeriodAsync(uow);
        await uow.SaveChangesAsync();

        var svc = CreateService(uow, new FakeCurrentUser { UserId = voter.Id });
        var result = await svc.CastAsync(idea.Id);

        Assert.False(result.Success);
        Assert.Contains("Only published or shortlisted", result.Message);
    }

    [Fact]
    public async Task Cast_forbids_voting_on_own_idea()
    {
        var (_, uow) = TestDb.Create();
        var voter = await AddUserAsync(uow, "Voter");
        var idea = await AddIdeaAsync(uow, voter.Id, IdeaStatus.Published);
        await AddPeriodAsync(uow);
        await uow.SaveChangesAsync();

        var svc = CreateService(uow, new FakeCurrentUser { UserId = voter.Id });
        await Assert.ThrowsAsync<ForbiddenException>(() => svc.CastAsync(idea.Id));
    }

    [Fact]
    public async Task Cast_fails_when_no_active_period_exists()
    {
        var (_, uow) = TestDb.Create();
        var creator = await AddUserAsync(uow, "Creator");
        var voter = await AddUserAsync(uow, "Voter");
        var idea = await AddIdeaAsync(uow, creator.Id, IdeaStatus.Published);
        await uow.SaveChangesAsync();

        var svc = CreateService(uow, new FakeCurrentUser { UserId = voter.Id });
        var result = await svc.CastAsync(idea.Id);

        Assert.False(result.Success);
        Assert.Equal("Voting is not open right now.", result.Message);
    }

    [Fact]
    public async Task Cast_fails_outside_the_voting_window()
    {
        var (_, uow) = TestDb.Create();
        var creator = await AddUserAsync(uow, "Creator");
        var voter = await AddUserAsync(uow, "Voter");
        var idea = await AddIdeaAsync(uow, creator.Id, IdeaStatus.Published);
        await AddPeriodAsync(uow, startAt: Now.AddDays(1), endAt: Now.AddDays(2));
        await uow.SaveChangesAsync();

        var svc = CreateService(uow, new FakeCurrentUser { UserId = voter.Id });
        var result = await svc.CastAsync(idea.Id);

        Assert.False(result.Success);
        Assert.Equal("The voting window is not currently open.", result.Message);
    }

    [Fact]
    public async Task Cast_forbids_suspended_accounts()
    {
        var (_, uow) = TestDb.Create();
        var creator = await AddUserAsync(uow, "Creator");
        var voter = await AddUserAsync(uow, "Voter", suspended: true);
        var idea = await AddIdeaAsync(uow, creator.Id, IdeaStatus.Published);
        await AddPeriodAsync(uow);
        await uow.SaveChangesAsync();

        var svc = CreateService(uow, new FakeCurrentUser { UserId = voter.Id });
        await Assert.ThrowsAsync<ForbiddenException>(() => svc.CastAsync(idea.Id));
    }

    [Fact]
    public async Task Cast_fails_when_verified_account_is_required_and_missing()
    {
        var (_, uow) = TestDb.Create();
        var creator = await AddUserAsync(uow, "Creator");
        var voter = await AddUserAsync(uow, "Voter", verified: false);
        var idea = await AddIdeaAsync(uow, creator.Id, IdeaStatus.Published);
        await AddPeriodAsync(uow, requiresVerified: true);
        await uow.SaveChangesAsync();

        var svc = CreateService(uow, new FakeCurrentUser { UserId = voter.Id });
        var result = await svc.CastAsync(idea.Id);

        Assert.False(result.Success);
        Assert.Equal("A verified account is required to vote.", result.Message);
    }

    [Fact]
    public async Task Cast_fails_for_accounts_younger_than_the_minimum_age()
    {
        var (_, uow) = TestDb.Create();
        var creator = await AddUserAsync(uow, "Creator");
        var voter = await AddUserAsync(uow, "Voter", createdAt: Now.AddDays(-10));
        var idea = await AddIdeaAsync(uow, creator.Id, IdeaStatus.Published);
        await AddPeriodAsync(uow, minAgeDays: 30);
        await uow.SaveChangesAsync();

        var svc = CreateService(uow, new FakeCurrentUser { UserId = voter.Id });
        var result = await svc.CastAsync(idea.Id);

        Assert.False(result.Success);
        Assert.Contains("at least 30 days old", result.Message);
    }

    [Fact]
    public async Task Cast_happy_path_counts_and_persists_vote()
    {
        var (_, uow) = TestDb.Create();
        var creator = await AddUserAsync(uow, "Creator");
        var voter = await AddUserAsync(uow, "Voter");
        var idea = await AddIdeaAsync(uow, creator.Id, IdeaStatus.Published);
        var period = await AddPeriodAsync(uow, visibility: VoteVisibility.Live);
        await uow.SaveChangesAsync();

        var svc = CreateService(uow, new FakeCurrentUser { UserId = voter.Id });
        var result = await svc.CastAsync(idea.Id);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.Voted);

        var stored = await uow.Repository<IdeaVote>().Query().Where(v => v.IdeaId == idea.Id && !v.IsDeleted).ToListAsync();
        var vote = Assert.Single(stored);
        Assert.Equal(voter.Id, vote.UserId);
        Assert.Equal(period.Id, vote.VotingPeriodId);
        Assert.True(vote.IsCounted);
    }

    [Fact]
    public async Task Cast_duplicate_vote_is_idempotent()
    {
        var (_, uow) = TestDb.Create();
        var creator = await AddUserAsync(uow, "Creator");
        var voter = await AddUserAsync(uow, "Voter");
        var idea = await AddIdeaAsync(uow, creator.Id, IdeaStatus.Shortlisted);
        await AddPeriodAsync(uow);
        await uow.SaveChangesAsync();

        var svc = CreateService(uow, new FakeCurrentUser { UserId = voter.Id });
        var first = await svc.CastAsync(idea.Id);
        var second = await svc.CastAsync(idea.Id);

        Assert.True(first.Success);
        Assert.True(second.Success);
        Assert.Equal(1, await uow.Repository<IdeaVote>().Query()
            .Where(v => v.IdeaId == idea.Id && !v.IsDeleted && v.IsCounted).CountAsync());
    }

    [Fact]
    public async Task Remove_not_voted_returns_failure()
    {
        var (_, uow) = TestDb.Create();
        var creator = await AddUserAsync(uow, "Creator");
        var voter = await AddUserAsync(uow, "Voter");
        var idea = await AddIdeaAsync(uow, creator.Id, IdeaStatus.Published);
        await AddPeriodAsync(uow);
        await uow.SaveChangesAsync();

        var svc = CreateService(uow, new FakeCurrentUser { UserId = voter.Id });
        var result = await svc.RemoveAsync(idea.Id);

        Assert.False(result.Success);
        Assert.Equal("You have not voted on this idea.", result.Message);
    }

    [Fact]
    public async Task Remove_soft_deletes_the_vote()
    {
        var (_, uow) = TestDb.Create();
        var creator = await AddUserAsync(uow, "Creator");
        var voter = await AddUserAsync(uow, "Voter");
        var idea = await AddIdeaAsync(uow, creator.Id, IdeaStatus.Published);
        await AddPeriodAsync(uow);
        await uow.SaveChangesAsync();

        var svc = CreateService(uow, new FakeCurrentUser { UserId = voter.Id });
        await svc.CastAsync(idea.Id);

        var result = await svc.RemoveAsync(idea.Id);

        Assert.True(result.Success);
        Assert.False(result.Data!.Voted);
        Assert.Equal(0, await uow.Repository<IdeaVote>().Query()
            .Where(v => v.IdeaId == idea.Id && !v.IsDeleted && v.IsCounted).CountAsync());
    }

    [Fact]
    public async Task GetVoteState_hides_counts_during_hidden_until_close_period()
    {
        var (_, uow) = TestDb.Create();
        var creator = await AddUserAsync(uow, "Creator");
        var voter = await AddUserAsync(uow, "Voter");
        var idea = await AddIdeaAsync(uow, creator.Id, IdeaStatus.Published);
        var period = await AddPeriodAsync(uow); // default HiddenUntilClose
        await uow.SaveChangesAsync();

        await uow.Repository<IdeaVote>().AddAsync(new IdeaVote
        {
            IdeaId = idea.Id,
            UserId = voter.Id,
            VotingPeriodId = period.Id,
            IsCounted = true,
            CreatedAt = Now
        });
        await uow.SaveChangesAsync();

        var svc = CreateService(uow, new FakeCurrentUser { UserId = voter.Id });
        var state = await svc.GetVoteStateAsync(idea.Id);

        Assert.Equal(1, await uow.Repository<IdeaVote>().Query()
            .Where(v => v.IdeaId == idea.Id && !v.IsDeleted && v.IsCounted).CountAsync());
        Assert.False(state.CountIsVisible);
        Assert.Equal(0, state.VoteCount);
        Assert.True(state.Voted);
    }

    [Fact]
    public async Task GetVoteState_shows_counts_for_live_or_closed_periods()
    {
        var (_, uow) = TestDb.Create();
        var creator = await AddUserAsync(uow, "Creator");
        var voter = await AddUserAsync(uow, "Voter");
        var idea = await AddIdeaAsync(uow, creator.Id, IdeaStatus.Published);
        var period = await AddPeriodAsync(uow, visibility: VoteVisibility.Live);
        await uow.SaveChangesAsync();

        await uow.Repository<IdeaVote>().AddAsync(new IdeaVote
        {
            IdeaId = idea.Id,
            UserId = voter.Id,
            VotingPeriodId = period.Id,
            IsCounted = true,
            CreatedAt = Now
        });
        await uow.SaveChangesAsync();

        var svc = CreateService(uow, new FakeCurrentUser { UserId = voter.Id });
        var state = await svc.GetVoteStateAsync(idea.Id);

        Assert.True(state.CountIsVisible);
        Assert.Equal(1, state.VoteCount);
    }

    // ---- Update / delete period (admin CRUD) ----

    [Fact]
    public async Task UpdatePeriod_requires_admin()
    {
        var (_, uow) = TestDb.Create();
        var period = await AddPeriodAsync(uow);
        await uow.SaveChangesAsync();

        var svc = CreateService(uow, new FakeCurrentUser { UserId = 1 });
        var request = new VotingPeriodCreateRequest
        {
            Name = "Renamed",
            StartAt = Now,
            EndAt = Now.AddDays(10),
            IsActive = true
        };

        await Assert.ThrowsAsync<ForbiddenException>(() => svc.UpdatePeriodAsync(period.Id, request));
    }

    [Fact]
    public async Task UpdatePeriod_updates_fields()
    {
        var (_, uow) = TestDb.Create();
        var period = await AddPeriodAsync(uow);
        await uow.SaveChangesAsync();

        var svc = CreateService(uow, new FakeCurrentUser { IsAdmin = true });
        var request = new VotingPeriodCreateRequest
        {
            Name = "Renamed",
            StartAt = Now,
            EndAt = Now.AddDays(10),
            Visibility = VoteVisibility.Live,
            MinimumAccountAgeDays = 3,
            RequiresVerifiedAccount = false,
            IsActive = false
        };

        var result = await svc.UpdatePeriodAsync(period.Id, request);

        Assert.True(result.Success);
        var stored = await uow.Repository<VotingPeriod>().Query().FirstAsync(vp => vp.Id == period.Id);
        Assert.Equal("Renamed", stored.Name);
        Assert.Equal(VoteVisibility.Live, stored.Visibility);
        Assert.Equal(3, stored.MinimumAccountAgeDays);
        Assert.False(stored.RequiresVerifiedAccount);
        Assert.False(stored.IsActive);
    }

    [Fact]
    public async Task UpdatePeriod_validates_window()
    {
        var (_, uow) = TestDb.Create();
        var period = await AddPeriodAsync(uow);
        await uow.SaveChangesAsync();

        var svc = CreateService(uow, new FakeCurrentUser { IsAdmin = true });
        var request = new VotingPeriodCreateRequest
        {
            Name = "Bad",
            StartAt = Now.AddDays(5),
            EndAt = Now,
            IsActive = true
        };

        var result = await svc.UpdatePeriodAsync(period.Id, request);

        Assert.False(result.Success);
        Assert.Equal("The period must end after it starts.", result.Message);
    }

    [Fact]
    public async Task UpdatePeriod_missing_period_throws_not_found()
    {
        var (_, uow) = TestDb.Create();
        var svc = CreateService(uow, new FakeCurrentUser { IsAdmin = true });

        await Assert.ThrowsAsync<NotFoundException>(() => svc.UpdatePeriodAsync(999,
            new VotingPeriodCreateRequest { Name = "X", StartAt = Now, EndAt = Now.AddDays(1) }));
    }

    [Fact]
    public async Task DeletePeriod_requires_admin()
    {
        var (_, uow) = TestDb.Create();
        var period = await AddPeriodAsync(uow);
        await uow.SaveChangesAsync();

        var svc = CreateService(uow, new FakeCurrentUser { UserId = 1 });
        await Assert.ThrowsAsync<ForbiddenException>(() => svc.DeletePeriodAsync(period.Id));
    }

    [Fact]
    public async Task DeletePeriod_with_votes_is_blocked()
    {
        var (_, uow) = TestDb.Create();
        var creator = await AddUserAsync(uow, "Creator");
        var voter = await AddUserAsync(uow, "Voter");
        var idea = await AddIdeaAsync(uow, creator.Id, IdeaStatus.Published);
        var period = await AddPeriodAsync(uow);
        await uow.SaveChangesAsync();

        await uow.Repository<IdeaVote>().AddAsync(new IdeaVote
        {
            IdeaId = idea.Id,
            UserId = voter.Id,
            VotingPeriodId = period.Id,
            IsCounted = true,
            CreatedAt = Now
        });
        await uow.SaveChangesAsync();

        var svc = CreateService(uow, new FakeCurrentUser { IsAdmin = true });
        var result = await svc.DeletePeriodAsync(period.Id);

        Assert.False(result.Success);
        Assert.Contains("recorded votes", result.Message);
        Assert.True(await uow.Repository<VotingPeriod>().Query().AnyAsync(vp => vp.Id == period.Id));
    }

    [Fact]
    public async Task DeletePeriod_without_votes_removes_period()
    {
        var (_, uow) = TestDb.Create();
        var period = await AddPeriodAsync(uow);
        await uow.SaveChangesAsync();

        var svc = CreateService(uow, new FakeCurrentUser { IsAdmin = true });
        var result = await svc.DeletePeriodAsync(period.Id);

        Assert.True(result.Success);
        Assert.False(await uow.Repository<VotingPeriod>().Query().AnyAsync(vp => vp.Id == period.Id));
    }
}