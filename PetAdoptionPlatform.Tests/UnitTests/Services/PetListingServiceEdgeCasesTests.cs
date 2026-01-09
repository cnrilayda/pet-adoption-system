using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PetAdoptionPlatform.Application.DTOs.Listings;
using PetAdoptionPlatform.Domain.Enums;
using PetAdoptionPlatform.Infrastructure.Services;
using PetAdoptionPlatform.Tests.TestUtilities;
using Xunit;

namespace PetAdoptionPlatform.Tests.UnitTests.Services;

public class PetListingServiceEdgeCasesTests
{
    [Fact]
    public async Task GetListingsAsync_ShouldFilterByCity()
    {
        using var ctx = TestDbContextFactory.Create();
        var owner = SeedHelper.CreateUser("owner@x.com");
        var l1 = SeedHelper.CreateListing(owner.Id);
        l1.City = "Istanbul";
        var l2 = SeedHelper.CreateListing(owner.Id);
        l2.City = "Ankara";
        await SeedHelper.SaveAsync(ctx, owner, l1, l2);

        var sut = new PetListingService(ctx);

        var result = (await sut.GetListingsAsync(new PetListingFilterDto { City = "Ankara" })).ToList();

        Assert.Single(result);
        Assert.Equal(l2.Id, result[0].Id);
    }

    [Fact]
    public async Task GetListingsAsync_ShouldFilterByType()
    {
        using var ctx = TestDbContextFactory.Create();
        var owner = SeedHelper.CreateUser("owner@x.com");
        var adopt = SeedHelper.CreateListing(owner.Id, ListingType.Adoption);
        var help = SeedHelper.CreateListing(owner.Id, ListingType.HelpRequest);
        await SeedHelper.SaveAsync(ctx, owner, adopt, help);

        var sut = new PetListingService(ctx);

        var result = (await sut.GetListingsAsync(new PetListingFilterDto { Type = ListingType.HelpRequest })).ToList();

        Assert.Single(result);
        Assert.Equal(help.Id, result[0].Id);
    }

    [Fact]
    public async Task GetListingsAsync_ShouldFilterBySearchTerm_InTitleOrDescription()
    {
        using var ctx = TestDbContextFactory.Create();
        var owner = SeedHelper.CreateUser("owner@x.com");
        var l1 = SeedHelper.CreateListing(owner.Id);
        l1.Title = "Cute cat";
        var l2 = SeedHelper.CreateListing(owner.Id);
        l2.Description = "This dog is friendly";
        await SeedHelper.SaveAsync(ctx, owner, l1, l2);

        var sut = new PetListingService(ctx);

        var result = (await sut.GetListingsAsync(new PetListingFilterDto { SearchTerm = "dog" })).ToList();

        Assert.Single(result);
        Assert.Equal(l2.Id, result[0].Id);
    }

    [Fact]
    public async Task GetListingsAsync_ShouldIncludePausedListings_WhenNoFilter()
    {
        // PetListingService does not exclude paused listings in GetListingsAsync.
        using var ctx = TestDbContextFactory.Create();
        var owner = SeedHelper.CreateUser("owner@x.com");
        var active = SeedHelper.CreateListing(owner.Id, isPaused: false);
        var paused = SeedHelper.CreateListing(owner.Id, isPaused: true);
        await SeedHelper.SaveAsync(ctx, owner, active, paused);

        var sut = new PetListingService(ctx);

        var result = (await sut.GetListingsAsync(new PetListingFilterDto())).ToList();

        Assert.Contains(result, r => r.Id == paused.Id);
        Assert.Contains(result, r => r.Id == active.Id);
    }

    [Fact]
    public async Task GetListingsAsync_ShouldPaginate()
    {
        using var ctx = TestDbContextFactory.Create();
        var owner = SeedHelper.CreateUser("owner@x.com");
        await SeedHelper.SaveAsync(ctx, owner);

        for (var i = 0; i < 25; i++)
        {
            var l = SeedHelper.CreateListing(owner.Id);
            l.Title = "L" + i;
            await SeedHelper.SaveAsync(ctx, l);
        }

        var sut = new PetListingService(ctx);

        var page2 = (await sut.GetListingsAsync(new PetListingFilterDto { Page = 2, PageSize = 20 })).ToList();

        Assert.Equal(5, page2.Count);
    }

    [Fact]
    public async Task TogglePauseListingAsync_ShouldToggleIsPaused()
    {
        using var ctx = TestDbContextFactory.Create();
        var owner = SeedHelper.CreateUser("owner@x.com");
        var listing = SeedHelper.CreateListing(owner.Id, isPaused: false);
        await SeedHelper.SaveAsync(ctx, owner, listing);

        var sut = new PetListingService(ctx);

        await sut.TogglePauseListingAsync(listing.Id, owner.Id);

        var refreshed = ctx.PetListings.Single(l => l.Id == listing.Id);
        Assert.True(refreshed.IsPaused);
    }

    [Fact]
    public async Task TogglePauseListingAsync_ShouldThrow_WhenNotOwner()
    {
        using var ctx = TestDbContextFactory.Create();
        var owner = SeedHelper.CreateUser("owner@x.com");
        var other = SeedHelper.CreateUser("other@x.com");
        var listing = SeedHelper.CreateListing(owner.Id, isPaused: false);
        await SeedHelper.SaveAsync(ctx, owner, other, listing);

        var sut = new PetListingService(ctx);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => sut.TogglePauseListingAsync(listing.Id, other.Id));
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

        // Deleted listings are hidden by global query filters; read with IgnoreQueryFilters().
        var refreshed = ctx.PetListings.IgnoreQueryFilters().Single(l => l.Id == listing.Id);
        Assert.True(refreshed.IsDeleted);
        Assert.False(refreshed.IsActive);
    }

    [Fact]
    public async Task UpdateListingAsync_ShouldUpdateProvidedFieldsOnly()
    {
        using var ctx = TestDbContextFactory.Create();
        var owner = SeedHelper.CreateUser("owner@x.com");
        var listing = SeedHelper.CreateListing(owner.Id);
        listing.Title = "Old";
        listing.Description = "OldD";
        await SeedHelper.SaveAsync(ctx, owner, listing);

        var sut = new PetListingService(ctx);

        var updated = await sut.UpdateListingAsync(listing.Id, new UpdatePetListingDto { Title = "New" }, owner.Id);

        Assert.Equal("New", updated.Title);
        Assert.Equal("OldD", updated.Description);
    }
}