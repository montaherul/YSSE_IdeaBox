using Microsoft.EntityFrameworkCore;
using YSSE.IdeaBox.Application.Abstractions;
using YSSE.IdeaBox.Application.Common;
using YSSE.IdeaBox.Application.Dtos;
using YSSE.IdeaBox.Application.Services.Engagement;
using YSSE.IdeaBox.Domain.Entities;
using YSSE.IdeaBox.Domain.Enums;

namespace YSSE.IdeaBox.Tests;

/// <summary>
/// Marketplace listing CRUD (Module I) driven through the real GenericRepository/UnitOfWork
/// over the EF InMemory provider. Ownership and admin rules are enforced server-side.
/// </summary>
public class MarketplaceServiceTests
{
    private static readonly DateTime Now = new(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc);

    private static MarketplaceService CreateService(IUnitOfWork uow, FakeCurrentUser current)
        => new(uow, current, new FakeClock(Now), new NoOpAuditWriter(), new NoOpNotificationDispatcher());

    private static async Task<MarketplaceListing> AddListingAsync(IUnitOfWork uow, int ownerId)
    {
        var listing = new MarketplaceListing
        {
            OwnerUserId = ownerId,
            Title = "Original listing",
            Description = "Original description",
            Category = "Books",
            Condition = "Used",
            ListingType = ListingType.Sale,
            Price = 100,
            Status = ListingStatus.Available,
            CreatedAt = Now
        };
        await uow.Repository<MarketplaceListing>().AddAsync(listing);
        return listing;
    }

    private static ListingCreateRequest NewRequest() => new()
    {
        Title = "Updated listing",
        Description = "Updated description",
        Category = "Electronics",
        Condition = "New",
        ListingType = ListingType.Sale,
        Price = 250,
        CoarseLocation = "Chattogram"
    };

    [Fact]
    public async Task Update_requires_authenticated_owner()
    {
        var (_, uow) = TestDb.Create();
        var listing = await AddListingAsync(uow, 1);
        await uow.SaveChangesAsync();

        var other = CreateService(uow, new FakeCurrentUser { UserId = 2 });
        await Assert.ThrowsAsync<ForbiddenException>(() => other.UpdateAsync(listing.Id, NewRequest()));
    }

    [Fact]
    public async Task Update_missing_listing_throws_not_found()
    {
        var (_, uow) = TestDb.Create();
        var svc = CreateService(uow, new FakeCurrentUser { UserId = 1 });

        await Assert.ThrowsAsync<NotFoundException>(() => svc.UpdateAsync(999, NewRequest()));
    }

    [Fact]
    public async Task Update_owner_can_edit_fields()
    {
        var (_, uow) = TestDb.Create();
        var listing = await AddListingAsync(uow, 1);
        await uow.SaveChangesAsync();

        var svc = CreateService(uow, new FakeCurrentUser { UserId = 1 });
        var result = await svc.UpdateAsync(listing.Id, NewRequest());

        Assert.True(result.Success);
        var stored = await uow.Repository<MarketplaceListing>().Query().FirstAsync(l => l.Id == listing.Id);
        Assert.Equal("Updated listing", stored.Title);
        Assert.Equal("Electronics", stored.Category);
        Assert.Equal(250, stored.Price);
        Assert.Equal("Chattogram", stored.CoarseLocation);
        Assert.Equal(Now, stored.UpdatedAt);
    }

    [Fact]
    public async Task Update_admin_can_edit_another_users_listing()
    {
        var (_, uow) = TestDb.Create();
        var listing = await AddListingAsync(uow, 2);
        await uow.SaveChangesAsync();

        var svc = CreateService(uow, new FakeCurrentUser { UserId = 1, IsAdmin = true });
        var result = await svc.UpdateAsync(listing.Id, NewRequest());

        Assert.True(result.Success);
        var stored = await uow.Repository<MarketplaceListing>().Query().FirstAsync(l => l.Id == listing.Id);
        Assert.Equal("Updated listing", stored.Title);
    }

    [Fact]
    public async Task Delete_requires_authenticated_owner()
    {
        var (_, uow) = TestDb.Create();
        var listing = await AddListingAsync(uow, 1);
        await uow.SaveChangesAsync();

        var svc = CreateService(uow, new FakeCurrentUser { UserId = 2 });
        await Assert.ThrowsAsync<ForbiddenException>(() => svc.DeleteAsync(listing.Id));
    }

    [Fact]
    public async Task Delete_missing_listing_throws_not_found()
    {
        var (_, uow) = TestDb.Create();
        var svc = CreateService(uow, new FakeCurrentUser { UserId = 1 });

        await Assert.ThrowsAsync<NotFoundException>(() => svc.DeleteAsync(999));
    }

    [Fact]
    public async Task Delete_soft_deletes_for_owner()
    {
        var (_, uow) = TestDb.Create();
        var listing = await AddListingAsync(uow, 1);
        await uow.SaveChangesAsync();

        var svc = CreateService(uow, new FakeCurrentUser { UserId = 1 });
        var result = await svc.DeleteAsync(listing.Id);

        Assert.True(result.Success);
        var stored = await uow.Repository<MarketplaceListing>().Query().FirstAsync(l => l.Id == listing.Id);
        Assert.True(stored.IsDeleted);
        Assert.Equal(Now, stored.DeletedAt);

        await Assert.ThrowsAsync<NotFoundException>(() => svc.GetByIdAsync(listing.Id));
    }

    [Fact]
    public async Task Get_by_id_hides_non_public_listing_from_other_users()
    {
        var (_, uow) = TestDb.Create();
        var listing = await AddListingAsync(uow, 1);
        listing.Status = ListingStatus.Draft;
        await uow.SaveChangesAsync();

        var svc = CreateService(uow, new FakeCurrentUser { UserId = 2 });
        await Assert.ThrowsAsync<NotFoundException>(() => svc.GetByIdAsync(listing.Id));
    }
}
