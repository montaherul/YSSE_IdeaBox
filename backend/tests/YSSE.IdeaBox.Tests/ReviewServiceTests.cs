using YSSE.IdeaBox.Application.Abstractions;
using YSSE.IdeaBox.Application.Common;
using YSSE.IdeaBox.Application.Services.Engagement;
using YSSE.IdeaBox.Domain.Entities;
using YSSE.IdeaBox.Domain.Enums;

namespace YSSE.IdeaBox.Tests;

/// <summary>
/// Community review aggregation (Module D): counts/averages only over published,
/// non-deleted reviews; pagination and ownership exposed through the real UoW path.
/// </summary>
public class ReviewServiceTests
{
    private static readonly DateTime Now = new(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc);

    private static ReviewService CreateService(IUnitOfWork uow, FakeCurrentUser current)
        => new(uow, current, new FakeClock(Now), new NoOpAuditWriter(), new NoOpNotificationDispatcher());

    private static async Task<Idea> SeedIdeaAsync(IUnitOfWork uow)
    {
        var idea = new Idea
        {
            Slug = "idea-" + Guid.NewGuid().ToString("N")[..6],
            Title = "Idea",
            ShortSummary = "Summary",
            CategoryId = 1,
            CreatorUserId = 99,
            Status = IdeaStatus.Published,
            PublishedAt = Now
        };
        await uow.Repository<Idea>().AddAsync(idea);
        await uow.SaveChangesAsync();
        return idea;
    }

    private static async Task<IdeaReview> SeedReviewAsync(IUnitOfWork uow, Idea idea,
        int? reviewerUserId, bool isGuest, int? rating,
        ReviewModerationStatus moderation = ReviewModerationStatus.Published, bool deleted = false,
        DateTime? createdAt = null)
    {
        var review = new IdeaReview
        {
            Idea = idea,
            IdeaId = idea.Id,
            ReviewerUserId = reviewerUserId,
            IsGuest = isGuest,
            Body = "Review",
            Rating = rating,
            ModerationStatus = moderation,
            IsDeleted = deleted,
            DeletedAt = deleted ? createdAt ?? Now : null,
            CreatedAt = createdAt ?? Now.AddDays(-1)
        };
        await uow.Repository<IdeaReview>().AddAsync(review);
        await uow.SaveChangesAsync();
        return review;
    }

    [Fact]
    public async Task GetCounts_includes_only_published_nondeleted_guest_and_registered()
    {
        var (_, uow) = TestDb.Create();

        var idea = await SeedIdeaAsync(uow);
        await SeedReviewAsync(uow, idea, null, true, 5, createdAt: Now.AddDays(-1));
        await SeedReviewAsync(uow, idea, 1, true, 4, ReviewModerationStatus.Hidden, createdAt: Now.AddDays(-2));
        await SeedReviewAsync(uow, idea, null, true, 2, deleted: true, createdAt: Now.AddDays(-3));
        await SeedReviewAsync(uow, idea, 2, false, 3, createdAt: Now.AddDays(-4));
        await SeedReviewAsync(uow, idea, 3, false, null, createdAt: Now.AddDays(-5));

        var svc = CreateService(uow, new FakeCurrentUser { UserId = 4 });
        var counts = await svc.GetCountsAsync(idea.Id);

        Assert.Equal(1, counts.AnonymousPublished);   // hidden and deleted guests excluded
        Assert.Equal(2, counts.RegisteredPublished);  // rated + unrated registered reviews
        Assert.Equal(4.0, counts.AverageRating);      // (5 + 3) / 2
    }

    [Fact]
    public async Task GetCounts_returns_null_average_when_no_ratings_exist()
    {
        var (_, uow) = TestDb.Create();

        var idea = await SeedIdeaAsync(uow);
        await SeedReviewAsync(uow, idea, 1, false, null);

        var svc = CreateService(uow, new FakeCurrentUser { UserId = 1 });
        var counts = await svc.GetCountsAsync(idea.Id);

        Assert.Equal(1, counts.RegisteredPublished);
        Assert.Null(counts.AverageRating);
    }

    [Fact]
    public async Task GetForIdea_is_paginated_and_ordered_newest_first()
    {
        var (_, uow) = TestDb.Create();

        var idea = await SeedIdeaAsync(uow);
        await SeedReviewAsync(uow, idea, 1, false, 4, createdAt: Now.AddDays(-1));
        await SeedReviewAsync(uow, idea, 2, false, 5, createdAt: Now.AddDays(-2));
        await SeedReviewAsync(uow, idea, 3, true, 3, createdAt: Now.AddDays(-3));
        await SeedReviewAsync(uow, idea, null, true, 2, ReviewModerationStatus.Hidden, createdAt: Now.AddDays(-4));
        await SeedReviewAsync(uow, idea, 4, true, 5, deleted: true, createdAt: Now.AddDays(-5));

        var svc = CreateService(uow, new FakeCurrentUser { UserId = 1 });
        var page1 = await svc.GetForIdeaAsync(idea.Id, 1, 2);

        Assert.Equal(3, page1.TotalCount);          // hidden + deleted excluded
        Assert.Equal(2, page1.Items.Count);
        Assert.True(page1.HasNext);
        Assert.Equal(4, page1.Items[0].Rating);     // newest review first
        Assert.True(page1.Items[0].CanEdit);        // author's own review
        Assert.False(page1.Items[1].CanEdit);

        var page2 = await svc.GetForIdeaAsync(idea.Id, 2, 2);
        Assert.Single(page2.Items);
        Assert.True(page2.Items[0].IsGuest);
    }

    [Fact]
    public async Task GetMine_returns_only_the_callers_reviews()
    {
        var (_, uow) = TestDb.Create();

        var idea = await SeedIdeaAsync(uow);
        await SeedReviewAsync(uow, idea, 1, false, 4, createdAt: Now.AddDays(-1));
        await SeedReviewAsync(uow, idea, 2, false, 5, createdAt: Now.AddDays(-2));
        await SeedReviewAsync(uow, idea, 2, false, 2, deleted: true, createdAt: Now.AddDays(-3));

        var svc = CreateService(uow, new FakeCurrentUser { UserId = 2 });
        var mine = await svc.GetMineAsync(1, 20);

        Assert.Single(mine.Items);                 // only user 2, non-deleted
        Assert.Equal(idea.Id, mine.Items[0].IdeaId);
        Assert.Equal("Idea", mine.Items[0].IdeaTitle);
    }

    [Fact]
    public async Task GetMine_requires_authentication()
    {
        var (_, uow) = TestDb.Create();
        var svc = CreateService(uow, new FakeCurrentUser());

        await Assert.ThrowsAsync<UnauthorizedException>(() => svc.GetMineAsync(1, 20));
    }
}