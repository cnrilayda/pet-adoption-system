using System;
using System.Linq;
using System.Threading.Tasks;
using PetAdoptionPlatform.Infrastructure.Services;
using PetAdoptionPlatform.Tests.TestUtilities;
using Xunit;

namespace PetAdoptionPlatform.Tests.UnitTests.Services;

public class FavoriteServiceTests
{
    [Fact]
    public async Task AddFavoriteAsync_ShouldCreateFavorite()
    {
        using var ctx = TestDbContextFactory.Create();
        var user = SeedHelper.CreateUser("u@x.com");
        var listing = SeedHelper.CreateListing(user.Id);
        await SeedHelper.SaveAsync(ctx, user, listing);

        var sut = new FavoriteService(ctx);

        var fav = await sut.AddFavoriteAsync(listing.Id, user.Id);

        Assert.Equal(listing.Id, fav.ListingId);
        Assert.True(await sut.IsFavoriteAsync(listing.Id, user.Id));
    }

    [Fact]
    public async Task AddFavoriteAsync_ShouldThrow_WhenAlreadyInFavorites()
    {
        using var ctx = TestDbContextFactory.Create();
        var user = SeedHelper.CreateUser("u@x.com");
        var listing = SeedHelper.CreateListing(user.Id);
        await SeedHelper.SaveAsync(ctx, user, listing);

        var sut = new FavoriteService(ctx);
        await sut.AddFavoriteAsync(listing.Id, user.Id);

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.AddFavoriteAsync(listing.Id, user.Id));
    }

    [Fact]
    public async Task RemoveFavoriteAsync_ShouldSoftDelete()
    {
        using var ctx = TestDbContextFactory.Create();
        var user = SeedHelper.CreateUser("u@x.com");
        var listing = SeedHelper.CreateListing(user.Id);
        await SeedHelper.SaveAsync(ctx, user, listing);

        var sut = new FavoriteService(ctx);
        await sut.AddFavoriteAsync(listing.Id, user.Id);

        await sut.RemoveFavoriteAsync(listing.Id, user.Id);

        Assert.False(await sut.IsFavoriteAsync(listing.Id, user.Id));
    }
}