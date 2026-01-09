using System;
using System.Linq;
using System.Threading.Tasks;
using PetAdoptionPlatform.Application.DTOs.Admin;
using PetAdoptionPlatform.Infrastructure.Services;
using PetAdoptionPlatform.Tests.TestUtilities;
using Xunit;

namespace PetAdoptionPlatform.Tests.UnitTests.Services;

public class AdminServiceTests
{
    [Fact]
    public async Task ApproveListingAsync_ShouldSetIsApprovedTrue()
    {
        using var ctx = TestDbContextFactory.Create();
        var owner = SeedHelper.CreateUser("owner@x.com");
        var listing = SeedHelper.CreateListing(owner.Id, isApproved:false);
        await SeedHelper.SaveAsync(ctx, owner, listing);

        var sut = new AdminService(ctx);

        await sut.ApproveListingAsync(listing.Id, new ApproveListingDto { IsApproved = true, AdminNotes = "ok" });

        var updated = ctx.PetListings.Single(l => l.Id == listing.Id);
        Assert.True(updated.IsApproved);
        Assert.NotNull(updated.UpdatedAt);
    }

    [Fact]
    public async Task GetPendingListingsAsync_ShouldReturnOnlyNotApproved()
    {
        using var ctx = TestDbContextFactory.Create();
        var owner = SeedHelper.CreateUser("owner@x.com");
        var l1 = SeedHelper.CreateListing(owner.Id, isApproved:false);
        var l2 = SeedHelper.CreateListing(owner.Id, isApproved:true);
        await SeedHelper.SaveAsync(ctx, owner, l1, l2);

        var sut = new AdminService(ctx);

        var pending = (await sut.GetPendingListingsAsync()).ToList();

        Assert.Single(pending);
        var first = pending[0];
        var idProp = first.GetType().GetProperty("Id");
        Assert.NotNull(idProp);
        var id = (System.Guid)idProp!.GetValue(first)!;
        Assert.Equal(l1.Id, id);

    }
}