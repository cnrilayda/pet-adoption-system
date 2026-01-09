using System.Threading.Tasks;
using PetAdoptionPlatform.Application.DTOs.Ratings;
using PetAdoptionPlatform.Domain.Enums;
using PetAdoptionPlatform.Infrastructure.Services;
using PetAdoptionPlatform.Tests.TestUtilities;
using Xunit;

namespace PetAdoptionPlatform.Tests.UnitTests.Services;

public class RatingServiceTests
{
    [Fact]
    public async Task CreateRatingAsync_ShouldThrow_WhenApplicationNotCompleted()
    {
        using var ctx = TestDbContextFactory.Create();
        var owner = SeedHelper.CreateUser("owner@x.com");
        var adopter = SeedHelper.CreateUser("adopter@x.com");
        var listing = SeedHelper.CreateListing(owner.Id, isApproved:true);
        var app = SeedHelper.CreateApplication(listing.Id, adopter.Id, ApplicationStatus.Accepted);

        await SeedHelper.SaveAsync(ctx, owner, adopter, listing, app);

        var sut = new RatingService(ctx);

        await Assert.ThrowsAsync<System.InvalidOperationException>(() =>
            sut.CreateRatingAsync(new CreateRatingDto { ApplicationId = app.Id, Score = 5, Comment = "great" }, adopter.Id));
    }

    [Fact]
    public async Task CreateRatingAsync_ShouldCreate_WhenCompleted_AndRaterIsAdopter()
    {
        using var ctx = TestDbContextFactory.Create();
        var owner = SeedHelper.CreateUser("owner@x.com");
        var adopter = SeedHelper.CreateUser("adopter@x.com");
        var listing = SeedHelper.CreateListing(owner.Id, isApproved:true);
        var app = SeedHelper.CreateApplication(listing.Id, adopter.Id, ApplicationStatus.Completed);

        await SeedHelper.SaveAsync(ctx, owner, adopter, listing, app);

        var sut = new RatingService(ctx);

        var dto = await sut.CreateRatingAsync(new CreateRatingDto { ApplicationId = app.Id, Score = 5, Comment = "great" }, adopter.Id);

        Assert.Equal(5, dto.Score);
        Assert.Equal(adopter.Id, dto.RaterId);
        Assert.Equal(owner.Id, dto.RatedUserId);
    }

    [Fact]
    public async Task CreateRatingAsync_ShouldUpdateExistingRating()
    {
        using var ctx = TestDbContextFactory.Create();
        var owner = SeedHelper.CreateUser("owner@x.com");
        var adopter = SeedHelper.CreateUser("adopter@x.com");
        var listing = SeedHelper.CreateListing(owner.Id, isApproved:true);
        var app = SeedHelper.CreateApplication(listing.Id, adopter.Id, ApplicationStatus.Completed);

        await SeedHelper.SaveAsync(ctx, owner, adopter, listing, app);

        var sut = new RatingService(ctx);

        var first = await sut.CreateRatingAsync(new CreateRatingDto { ApplicationId = app.Id, Score = 3, Comment = "ok" }, adopter.Id);
        var second = await sut.CreateRatingAsync(new CreateRatingDto { ApplicationId = app.Id, Score = 4, Comment = "better" }, adopter.Id);

        Assert.Equal(first.Id, second.Id);
        Assert.Equal(4, second.Score);
    }

    [Fact]
    public async Task CreateRatingAsync_ShouldThrow_WhenRaterNotInvolved()
    {
        using var ctx = TestDbContextFactory.Create();
        var owner = SeedHelper.CreateUser("owner@x.com");
        var adopter = SeedHelper.CreateUser("adopter@x.com");
        var other = SeedHelper.CreateUser("other@x.com");
        var listing = SeedHelper.CreateListing(owner.Id, isApproved:true);
        var app = SeedHelper.CreateApplication(listing.Id, adopter.Id, ApplicationStatus.Completed);

        await SeedHelper.SaveAsync(ctx, owner, adopter, other, listing, app);

        var sut = new RatingService(ctx);

        await Assert.ThrowsAsync<System.UnauthorizedAccessException>(() =>
            sut.CreateRatingAsync(new CreateRatingDto { ApplicationId = app.Id, Score = 5 }, other.Id));
    }
}
