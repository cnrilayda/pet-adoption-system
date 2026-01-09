using System.Linq;
using System.Threading.Tasks;
using PetAdoptionPlatform.Application.DTOs.Applications;
using PetAdoptionPlatform.Domain.Enums;
using PetAdoptionPlatform.Infrastructure.Services;
using PetAdoptionPlatform.Tests.TestUtilities;
using Xunit;

namespace PetAdoptionPlatform.Tests.UnitTests.Services;

public class ApplicationServiceTests
{
    [Fact]
    public async Task CreateApplicationAsync_ShouldThrow_WhenListingNotApproved()
    {
        using var ctx = TestDbContextFactory.Create();
        var owner = SeedHelper.CreateUser("owner@x.com");
        var adopter = SeedHelper.CreateUser("adopter@x.com");
        var listing = SeedHelper.CreateListing(owner.Id, isApproved:false);
        await SeedHelper.SaveAsync(ctx, owner, adopter, listing);

        var sut = new ApplicationService(ctx);

        await Assert.ThrowsAsync<System.InvalidOperationException>(() =>
            sut.CreateApplicationAsync(new CreateApplicationDto { ListingId = listing.Id, Message = "hi" }, adopter.Id));
    }

    [Fact]
    public async Task CreateApplicationAsync_ShouldRequireEligibilityForm_ForAdoptionListing()
    {
        using var ctx = TestDbContextFactory.Create();
        var owner = SeedHelper.CreateUser("owner@x.com");
        var adopter = SeedHelper.CreateUser("adopter@x.com");
        var listing = SeedHelper.CreateListing(owner.Id, isApproved:true, isPaused:false);
        await SeedHelper.SaveAsync(ctx, owner, adopter, listing);

        var sut = new ApplicationService(ctx);

        await Assert.ThrowsAsync<System.InvalidOperationException>(() =>
            sut.CreateApplicationAsync(new CreateApplicationDto { ListingId = listing.Id, Message = "hi" }, adopter.Id));
    }

    [Fact]
    public async Task CreateApplicationAsync_ShouldCreatePending_WhenEligibilityExists()
    {
        using var ctx = TestDbContextFactory.Create();
        var owner = SeedHelper.CreateUser("owner@x.com");
        var adopter = SeedHelper.CreateUser("adopter@x.com");
        var listing = SeedHelper.CreateListing(owner.Id, isApproved:true, isPaused:false);
        var form = SeedHelper.CreateEligibilityForm(adopter.Id);
        await SeedHelper.SaveAsync(ctx, owner, adopter, listing, form);

        var sut = new ApplicationService(ctx);

        var created = await sut.CreateApplicationAsync(new CreateApplicationDto { ListingId = listing.Id, Message = "hi" }, adopter.Id);

        Assert.Equal(ApplicationStatus.Pending, created.Status);
        Assert.Equal(listing.Id, created.ListingId);
    }

    [Fact]
    public async Task CreateApplicationAsync_ShouldThrow_WhenDuplicateApplication()
    {
        using var ctx = TestDbContextFactory.Create();
        var owner = SeedHelper.CreateUser("owner@x.com");
        var adopter = SeedHelper.CreateUser("adopter@x.com");
        var listing = SeedHelper.CreateListing(owner.Id, isApproved:true, isPaused:false);
        var form = SeedHelper.CreateEligibilityForm(adopter.Id);
        var existing = SeedHelper.CreateApplication(listing.Id, adopter.Id);
        await SeedHelper.SaveAsync(ctx, owner, adopter, listing, form, existing);

        var sut = new ApplicationService(ctx);

        await Assert.ThrowsAsync<System.InvalidOperationException>(() =>
            sut.CreateApplicationAsync(new CreateApplicationDto { ListingId = listing.Id, Message = "again" }, adopter.Id));
    }

    [Fact]
    public async Task UpdateApplicationStatusAsync_Accepted_ShouldRejectOtherPendingAndPauseListing()
    {
        using var ctx = TestDbContextFactory.Create();
        var owner = SeedHelper.CreateUser("owner@x.com");
        var adopter1 = SeedHelper.CreateUser("a1@x.com");
        var adopter2 = SeedHelper.CreateUser("a2@x.com");
        var listing = SeedHelper.CreateListing(owner.Id, isApproved:true, isPaused:false);

        var form1 = SeedHelper.CreateEligibilityForm(adopter1.Id);
        var form2 = SeedHelper.CreateEligibilityForm(adopter2.Id);

        var app1 = SeedHelper.CreateApplication(listing.Id, adopter1.Id, ApplicationStatus.UnderReview);
        var app2 = SeedHelper.CreateApplication(listing.Id, adopter2.Id, ApplicationStatus.Pending);

        await SeedHelper.SaveAsync(ctx, owner, adopter1, adopter2, listing, form1, form2, app1, app2);

        var sut = new ApplicationService(ctx);

        var updated = await sut.UpdateApplicationStatusAsync(app1.Id, new UpdateApplicationStatusDto
        {
            Status = ApplicationStatus.Accepted,
            AdminNotes = "ok"
        }, owner.Id);

        Assert.Equal(ApplicationStatus.Accepted, updated.Status);

        var refreshedApp2 = ctx.AdoptionApplications.Single(a => a.Id == app2.Id);
        Assert.Equal(ApplicationStatus.Rejected, refreshedApp2.Status);

        var refreshedListing = ctx.PetListings.Single(l => l.Id == listing.Id);
        Assert.True(refreshedListing.IsPaused);
    }

    [Fact]
    public async Task CancelApplicationAsync_ShouldSetCancelled_ForPending()
    {
        using var ctx = TestDbContextFactory.Create();
        var owner = SeedHelper.CreateUser("owner@x.com");
        var adopter = SeedHelper.CreateUser("a@x.com");
        var listing = SeedHelper.CreateListing(owner.Id, isApproved:true, isPaused:false);
        var app = SeedHelper.CreateApplication(listing.Id, adopter.Id, ApplicationStatus.Pending);
        await SeedHelper.SaveAsync(ctx, owner, adopter, listing, app);

        var sut = new ApplicationService(ctx);
        await sut.CancelApplicationAsync(app.Id, adopter.Id);

        var refreshed = ctx.AdoptionApplications.Single(a => a.Id == app.Id);
        Assert.Equal(ApplicationStatus.Cancelled, refreshed.Status);
    }

    [Fact]
    public async Task GetListingApplicationsAsync_ShouldThrow_WhenNotOwner()
    {
        using var ctx = TestDbContextFactory.Create();
        var owner = SeedHelper.CreateUser("owner@x.com");
        var other = SeedHelper.CreateUser("other@x.com");
        var listing = SeedHelper.CreateListing(owner.Id);
        await SeedHelper.SaveAsync(ctx, owner, other, listing);

        var sut = new ApplicationService(ctx);

        await Assert.ThrowsAsync<System.UnauthorizedAccessException>(() => sut.GetListingApplicationsAsync(listing.Id, other.Id));
    }
[Fact]
public async Task UpdateApplicationStatusAsync_ShouldThrow_WhenUserNotOwnerOrAdmin()
{
    using var ctx = TestDbContextFactory.Create();
    var owner = SeedHelper.CreateUser("owner@x.com");
    var adopter = SeedHelper.CreateUser("adopter@x.com");
    var other = SeedHelper.CreateUser("other@x.com");
    var listing = SeedHelper.CreateListing(owner.Id, isApproved:true, isPaused:false);
    var app = SeedHelper.CreateApplication(listing.Id, adopter.Id, ApplicationStatus.Pending);
    await SeedHelper.SaveAsync(ctx, owner, adopter, other, listing, app);

    var sut = new ApplicationService(ctx);

    await Assert.ThrowsAsync<System.UnauthorizedAccessException>(() =>
        sut.UpdateApplicationStatusAsync(app.Id, new UpdateApplicationStatusDto { Status = ApplicationStatus.UnderReview }, other.Id));
}

[Fact]
public async Task UpdateApplicationStatusAsync_ShouldThrow_OnInvalidTransition()
{
    using var ctx = TestDbContextFactory.Create();
    var owner = SeedHelper.CreateUser("owner@x.com");
    var adopter = SeedHelper.CreateUser("adopter@x.com");
    var listing = SeedHelper.CreateListing(owner.Id, isApproved:true, isPaused:false);
    var app = SeedHelper.CreateApplication(listing.Id, adopter.Id, ApplicationStatus.Rejected);
    await SeedHelper.SaveAsync(ctx, owner, adopter, listing, app);

    var sut = new ApplicationService(ctx);

    await Assert.ThrowsAsync<System.InvalidOperationException>(() =>
        sut.UpdateApplicationStatusAsync(app.Id, new UpdateApplicationStatusDto { Status = ApplicationStatus.Accepted }, owner.Id));
}

[Fact]
public async Task CancelApplicationAsync_ShouldThrow_WhenNotOwner()
{
    using var ctx = TestDbContextFactory.Create();
    var owner = SeedHelper.CreateUser("owner@x.com");
    var adopter = SeedHelper.CreateUser("adopter@x.com");
    var other = SeedHelper.CreateUser("other@x.com");
    var listing = SeedHelper.CreateListing(owner.Id, isApproved:true, isPaused:false);
    var app = SeedHelper.CreateApplication(listing.Id, adopter.Id, ApplicationStatus.Pending);
    await SeedHelper.SaveAsync(ctx, owner, adopter, other, listing, app);

    var sut = new ApplicationService(ctx);

    await Assert.ThrowsAsync<System.UnauthorizedAccessException>(() => sut.CancelApplicationAsync(app.Id, other.Id));
}

[Fact]
public async Task CancelApplicationAsync_ShouldThrow_WhenStatusNotCancelable()
{
    using var ctx = TestDbContextFactory.Create();
    var owner = SeedHelper.CreateUser("owner@x.com");
    var adopter = SeedHelper.CreateUser("adopter@x.com");
    var listing = SeedHelper.CreateListing(owner.Id, isApproved:true, isPaused:false);
    var app = SeedHelper.CreateApplication(listing.Id, adopter.Id, ApplicationStatus.Accepted);
    await SeedHelper.SaveAsync(ctx, owner, adopter, listing, app);

    var sut = new ApplicationService(ctx);

    await Assert.ThrowsAsync<System.InvalidOperationException>(() => sut.CancelApplicationAsync(app.Id, adopter.Id));
}
}
