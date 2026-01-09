using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PetAdoptionPlatform.Infrastructure.Services;
using PetAdoptionPlatform.Tests.TestUtilities;
using Xunit;

namespace PetAdoptionPlatform.Tests.UnitTests.Services;

public class FavoriteServiceAdditionalTests
{
    [Fact]
    public async Task RemoveFavoriteAsync_ShouldRemove_WhenExists()
    {
        using var ctx = TestDbContextFactory.Create();
        var user = SeedHelper.CreateUser("u@x.com");
        var owner = SeedHelper.CreateUser("o@x.com");
        var listing = SeedHelper.CreateListing(owner.Id);
        await SeedHelper.SaveAsync(ctx, user, owner, listing);

        var sut = new FavoriteService(ctx);
        await sut.AddFavoriteAsync(listing.Id, user.Id);

        await sut.RemoveFavoriteAsync(listing.Id, user.Id);

        Assert.False(await sut.IsFavoriteAsync(listing.Id, user.Id));
    }

    [Fact]
    public async Task RemoveFavoriteAsync_ShouldThrow_WhenNotFound()
    {
        using var ctx = TestDbContextFactory.Create();
        var sut = new FavoriteService(ctx);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => sut.RemoveFavoriteAsync(Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public async Task GetUserFavoritesAsync_ShouldReturnOnlyUsersFavorites()
    {
        using var ctx = TestDbContextFactory.Create();
        var u1 = SeedHelper.CreateUser("u1@x.com");
        var u2 = SeedHelper.CreateUser("u2@x.com");
        var owner = SeedHelper.CreateUser("o@x.com");
        var l1 = SeedHelper.CreateListing(owner.Id);
        var l2 = SeedHelper.CreateListing(owner.Id);
        await SeedHelper.SaveAsync(ctx, u1, u2, owner, l1, l2);

        var sut = new FavoriteService(ctx);
        await sut.AddFavoriteAsync(l1.Id, u1.Id);
        await sut.AddFavoriteAsync(l2.Id, u2.Id);

        var favorites = (await sut.GetUserFavoritesAsync(u1.Id)).ToList();

        Assert.Single(favorites);
        Assert.Equal(l1.Id, favorites[0].ListingId);
    }

    [Fact]
    public async Task AddFavoriteAsync_ShouldThrow_WhenListingNotFound()
    {
        using var ctx = TestDbContextFactory.Create();
        var u1 = SeedHelper.CreateUser("u1@x.com");
        await SeedHelper.SaveAsync(ctx, u1);

        var sut = new FavoriteService(ctx);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => sut.AddFavoriteAsync(Guid.NewGuid(), u1.Id));
    }

    [Fact]
    public async Task AddFavoriteAsync_ShouldAllow_WhenUserInactive()
    {
        // FavoriteService does not enforce "IsActive"; it only validates that the user exists.
        using var ctx = TestDbContextFactory.Create();
        var u1 = SeedHelper.CreateUser("u1@x.com", isActive: false);
        var owner = SeedHelper.CreateUser("o@x.com");
        var listing = SeedHelper.CreateListing(owner.Id);
        await SeedHelper.SaveAsync(ctx, u1, owner, listing);

        var sut = new FavoriteService(ctx);

        await sut.AddFavoriteAsync(listing.Id, u1.Id);

        Assert.True(await sut.IsFavoriteAsync(listing.Id, u1.Id));
    }
}
