using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using PetAdoptionPlatform.Application.DTOs.Donations;
using PetAdoptionPlatform.Application.Interfaces;
using PetAdoptionPlatform.Domain.Enums;
using PetAdoptionPlatform.Infrastructure.Services;
using PetAdoptionPlatform.Tests.TestUtilities;
using Xunit;

namespace PetAdoptionPlatform.Tests.UnitTests.Services;

public class DonationServiceAdditionalTests
{
    [Fact]
    public async Task CreateDonationAsync_ShouldSetDonorNameAnonymous_WhenIsAnonymousTrue()
    {
        using var ctx = TestDbContextFactory.Create();
        var donor = SeedHelper.CreateUser("donor@x.com");
        await SeedHelper.SaveAsync(ctx, donor);

        var gateway = new Mock<IPaymentGateway>();
        gateway.Setup(g => g.ProcessPaymentAsync(10m, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentResult { Success = true, TransactionId = "tx", Status = "ok" });

        var sut = new DonationService(ctx, gateway.Object);

        var dto = await sut.CreateDonationAsync(new CreateDonationDto { Amount = 10m, IsAnonymous = true }, donor.Id);

        Assert.True(dto.IsAnonymous);
        Assert.Equal("Anonymous", dto.DonorName);
    }

    [Fact]
    public async Task CreateDonationAsync_ShouldThrow_WhenDonorInactive()
    {
        using var ctx = TestDbContextFactory.Create();
        var donor = SeedHelper.CreateUser("donor@x.com", isActive: false);
        await SeedHelper.SaveAsync(ctx, donor);

        var gateway = new Mock<IPaymentGateway>();
        gateway.Setup(g => g.ProcessPaymentAsync(It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentResult { Success = true, TransactionId = "tx", Status = "ok" });

        var sut = new DonationService(ctx, gateway.Object);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            sut.CreateDonationAsync(new CreateDonationDto { Amount = 10m }, donor.Id));
    }

    [Fact]
    public async Task CreateDonationAsync_ShouldThrow_WhenListingNotFound()
    {
        using var ctx = TestDbContextFactory.Create();
        var donor = SeedHelper.CreateUser("donor@x.com");
        await SeedHelper.SaveAsync(ctx, donor);

        var gateway = new Mock<IPaymentGateway>();
        gateway.Setup(g => g.ProcessPaymentAsync(It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentResult { Success = true, TransactionId = "tx", Status = "ok" });

        var sut = new DonationService(ctx, gateway.Object);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            sut.CreateDonationAsync(new CreateDonationDto { Amount = 10m, ListingId = Guid.NewGuid() }, donor.Id));
    }

    [Fact]
    public async Task CreateDonationAsync_ShouldAllowDonation_EvenIfListingNotApproved()
    {
        // DonationService does NOT validate listing approval; it only validates ListingType==HelpRequest.
        using var ctx = TestDbContextFactory.Create();
        var owner = SeedHelper.CreateUser("owner@x.com");
        var donor = SeedHelper.CreateUser("donor@x.com");
        var listing = SeedHelper.CreateListing(owner.Id, ListingType.HelpRequest, isApproved: false);
        listing.CollectedAmount = 0m;

        await SeedHelper.SaveAsync(ctx, owner, donor, listing);

        var gateway = new Mock<IPaymentGateway>();
        gateway.Setup(g => g.ProcessPaymentAsync(It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentResult { Success = true, TransactionId = "tx", Status = "ok" });

        var sut = new DonationService(ctx, gateway.Object);

        await sut.CreateDonationAsync(new CreateDonationDto { Amount = 10m, ListingId = listing.Id }, donor.Id);

        var refreshed = ctx.PetListings.Single(l => l.Id == listing.Id);
        Assert.Equal(10m, refreshed.CollectedAmount);
    }

    [Fact]
    public async Task GetDonationByIdAsync_ShouldThrow_WhenNotFound()
    {
        using var ctx = TestDbContextFactory.Create();
        var sut = new DonationService(ctx, Mock.Of<IPaymentGateway>());

        await Assert.ThrowsAsync<KeyNotFoundException>(() => sut.GetDonationByIdAsync(Guid.NewGuid()));
    }
}