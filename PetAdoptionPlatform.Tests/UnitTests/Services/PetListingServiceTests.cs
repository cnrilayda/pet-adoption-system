using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using PetAdoptionPlatform.Application.DTOs.Listings;
using PetAdoptionPlatform.Domain.Enums;
using PetAdoptionPlatform.Infrastructure.Services;
using PetAdoptionPlatform.Tests.TestUtilities;
using Xunit;

namespace PetAdoptionPlatform.Tests.UnitTests.Services;

public class PetListingServiceTests
{
    [Fact]
    public async Task CreateListingAsync_ShouldSetIsApprovedFalse_ByDefault()
    {
        using var ctx = TestDbContextFactory.Create();
        var user = SeedHelper.CreateUser("owner@x.com");
        await SeedHelper.SaveAsync(ctx, user);

        var sut = new PetListingService(ctx);

        var dto = new CreatePetListingDto
        {
            Type = ListingType.Adoption,
            Title = "T",
            Description = "D",
            City = "Istanbul",
            District = "Kadikoy"
        };

        var result = await sut.CreateListingAsync(dto, user.Id);

        var listing = ctx.PetListings.Single(l => l.Id == result.Id);
        Assert.False(listing.IsApproved);
        Assert.Equal(user.Id, listing.OwnerId);
    }

    [Fact]
    public async Task CreateListingAsync_ShouldCreatePhotos_AndMarkFirstPrimary()
    {
        using var ctx = TestDbContextFactory.Create();
        var user = SeedHelper.CreateUser("owner@x.com");
        await SeedHelper.SaveAsync(ctx, user);

        var sut = new PetListingService(ctx);

        var dto = new CreatePetListingDto
        {
            Type = ListingType.Adoption,
            Title = "T",
            Description = "D",
            City = "Istanbul",
            District = "Kadikoy",
            PhotoUrls = new() { "u1", "u2" }
        };

        var result = await sut.CreateListingAsync(dto, user.Id);

        var photos = ctx.PetPhotos.Where(p => p.ListingId == result.Id).OrderBy(p => p.DisplayOrder).ToList();
        Assert.Equal(2, photos.Count);
        Assert.True(photos[0].IsPrimary);
        Assert.False(photos[1].IsPrimary);
    }

    [Fact]
    public async Task CreateListingAsync_ShouldThrowUnauthorized_WhenUserInactive()
    {
        using var ctx = TestDbContextFactory.Create();
        var user = SeedHelper.CreateUser("owner@x.com", isActive:false);
        await SeedHelper.SaveAsync(ctx, user);

        var sut = new PetListingService(ctx);

        await Assert.ThrowsAsync<System.UnauthorizedAccessException>(() =>
            sut.CreateListingAsync(new CreatePetListingDto
            {
                Type = ListingType.Adoption,
                Title = "T",
                Description = "D",
                City = "Istanbul",
                District = "Kadikoy"
            }, user.Id));
    }

    [Fact]
    public async Task GetListingByIdAsync_ShouldReturnCounts()
    {
        using var ctx = TestDbContextFactory.Create();
        var owner = SeedHelper.CreateUser("owner@x.com");
        var applicant = SeedHelper.CreateUser("app@x.com");
        var listing = SeedHelper.CreateListing(owner.Id, isApproved:true);
        var app = SeedHelper.CreateApplication(listing.Id, applicant.Id);
        await SeedHelper.SaveAsync(ctx, owner, applicant, listing, app);

        var sut = new PetListingService(ctx);
        var dto = await sut.GetListingByIdAsync(listing.Id);

        Assert.Equal(1, dto.ApplicationCount);
        Assert.Equal(0, dto.FavoriteCount);
    }

    [Fact]
    public async Task TogglePauseListingAsync_ShouldToggle_WhenOwner()
    {
        using var ctx = TestDbContextFactory.Create();
        var owner = SeedHelper.CreateUser("owner@x.com");
        var listing = SeedHelper.CreateListing(owner.Id, isApproved:true, isPaused:false);
        await SeedHelper.SaveAsync(ctx, owner, listing);

        var sut = new PetListingService(ctx);

        await sut.TogglePauseListingAsync(listing.Id, owner.Id);

        var dto = await sut.GetListingByIdAsync(listing.Id);
        Assert.True(dto.IsPaused);
    }

    [Fact]
    public async Task TogglePauseListingAsync_ShouldThrow_WhenNotOwner()
    {
        using var ctx = TestDbContextFactory.Create();
        var owner = SeedHelper.CreateUser("owner@x.com");
        var other = SeedHelper.CreateUser("other@x.com");
        var listing = SeedHelper.CreateListing(owner.Id);
        await SeedHelper.SaveAsync(ctx, owner, other, listing);

        var sut = new PetListingService(ctx);

        await Assert.ThrowsAsync<System.UnauthorizedAccessException>(() => sut.TogglePauseListingAsync(listing.Id, other.Id));
    }

    [Fact]
    public async Task GetUserListingsAsync_ShouldReturnOnlyOwnersListings()
    {
        using var ctx = TestDbContextFactory.Create();
        var u1 = SeedHelper.CreateUser("u1@x.com");
        var u2 = SeedHelper.CreateUser("u2@x.com");
        var l1 = SeedHelper.CreateListing(u1.Id);
        var l2 = SeedHelper.CreateListing(u2.Id);
        await SeedHelper.SaveAsync(ctx, u1, u2, l1, l2);

        var sut = new PetListingService(ctx);

        var list = (await sut.GetUserListingsAsync(u1.Id)).ToList();

        Assert.Single(list);
        Assert.Equal(l1.Id, list[0].Id);
    }
[Fact]
public async Task UpdateListingAsync_ShouldUpdateFields_WhenOwner()
{
    using var ctx = TestDbContextFactory.Create();
    var owner = SeedHelper.CreateUser("owner@x.com");
    var listing = SeedHelper.CreateListing(owner.Id);
    await SeedHelper.SaveAsync(ctx, owner, listing);

    var sut = new PetListingService(ctx);

    var updated = await sut.UpdateListingAsync(listing.Id, new UpdatePetListingDto
    {
        Title = "NewTitle",
        Description = "NewDesc"
    }, owner.Id);

    Assert.Equal("NewTitle", updated.Title);
    Assert.Equal("NewDesc", updated.Description);
}

[Fact]
public async Task UpdateListingAsync_ShouldThrow_WhenNotOwner()
{
    using var ctx = TestDbContextFactory.Create();
    var owner = SeedHelper.CreateUser("owner@x.com");
    var other = SeedHelper.CreateUser("other@x.com");
    var listing = SeedHelper.CreateListing(owner.Id);
    await SeedHelper.SaveAsync(ctx, owner, other, listing);

    var sut = new PetListingService(ctx);

    await Assert.ThrowsAsync<System.UnauthorizedAccessException>(() =>
        sut.UpdateListingAsync(listing.Id, new UpdatePetListingDto { Title = "x" }, other.Id));
}

[Fact]
public async Task DeleteListingAsync_ShouldSoftDelete()
{
    using var ctx = TestDbContextFactory.Create();
    var owner = SeedHelper.CreateUser("owner@x.com");
    var listing = SeedHelper.CreateListing(owner.Id);
    await SeedHelper.SaveAsync(ctx, owner, listing);

    var sut = new PetListingService(ctx);

    await sut.DeleteListingAsync(listing.Id, owner.Id);

    // query filter hides it, so use IgnoreQueryFilters
    var deleted = ctx.PetListings.IgnoreQueryFilters().Single(l => l.Id == listing.Id);
    Assert.True(deleted.IsDeleted);
}

[Fact]
public async Task DeleteListingAsync_ShouldThrow_WhenNotOwner()
{
    using var ctx = TestDbContextFactory.Create();
    var owner = SeedHelper.CreateUser("owner@x.com");
    var other = SeedHelper.CreateUser("other@x.com");
    var listing = SeedHelper.CreateListing(owner.Id);
    await SeedHelper.SaveAsync(ctx, owner, other, listing);

    var sut = new PetListingService(ctx);

    await Assert.ThrowsAsync<System.UnauthorizedAccessException>(() => sut.DeleteListingAsync(listing.Id, other.Id));
}
}
