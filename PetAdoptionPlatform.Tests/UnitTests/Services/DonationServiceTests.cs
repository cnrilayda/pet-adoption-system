using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using PetAdoptionPlatform.Application.DTOs.Donations;
using PetAdoptionPlatform.Application.Interfaces;
using PetAdoptionPlatform.Infrastructure.Services;
using PetAdoptionPlatform.Tests.TestUtilities;
using Xunit;

namespace PetAdoptionPlatform.Tests.UnitTests.Services;

public class DonationServiceTests
{
    [Fact]
    public async Task CreateDonationAsync_ShouldPersistDonation_WhenPaymentSuccess()
    {
        using var ctx = TestDbContextFactory.Create();
        var donor = SeedHelper.CreateUser("donor@x.com");
        await SeedHelper.SaveAsync(ctx, donor);

        var gateway = new Mock<IPaymentGateway>();
        gateway.Setup(g => g.ProcessPaymentAsync(10m, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentResult { Success = true, TransactionId = "tx", Status = "ok" });

        var sut = new DonationService(ctx, gateway.Object);

        var dto = await sut.CreateDonationAsync(new CreateDonationDto { Amount = 10m, Message = "hi" }, donor.Id);

        Assert.Equal(10m, dto.Amount);
        Assert.Equal(donor.Id, dto.DonorId);
    }

    [Fact]
    public async Task CreateDonationAsync_ShouldThrow_WhenPaymentFails()
    {
        using var ctx = TestDbContextFactory.Create();
        var donor = SeedHelper.CreateUser("donor@x.com");
        await SeedHelper.SaveAsync(ctx, donor);

        var gateway = new Mock<IPaymentGateway>();
        gateway.Setup(g => g.ProcessPaymentAsync(10m, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentResult { Success = false, ErrorMessage = "declined" });

        var sut = new DonationService(ctx, gateway.Object);

        await Assert.ThrowsAsync<System.InvalidOperationException>(() =>
            sut.CreateDonationAsync(new CreateDonationDto { Amount = 10m, Message = "hi" }, donor.Id));
    }
    [Fact]
    public async Task CreateDonationAsync_ShouldIncrementListingCollectedAmount_WhenListingDonation()
    {
        using var ctx = TestDbContextFactory.Create();
        var owner = SeedHelper.CreateUser("owner@x.com");
        var donor = SeedHelper.CreateUser("donor@x.com");
        var listing = SeedHelper.CreateListing(owner.Id, isApproved: true);
        listing.Type = PetAdoptionPlatform.Domain.Enums.ListingType.HelpRequest;
        listing.CollectedAmount = 5m;


        await SeedHelper.SaveAsync(ctx, owner, donor, listing);

        var gateway = new Mock<IPaymentGateway>();
        gateway.Setup(g => g.ProcessPaymentAsync(10m, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentResult { Success = true, TransactionId = "tx", Status = "ok" });

        var sut = new DonationService(ctx, gateway.Object);

        await sut.CreateDonationAsync(new CreateDonationDto { Amount = 10m, ListingId = listing.Id }, donor.Id);

        var refreshed = ctx.PetListings.Single(l => l.Id == listing.Id);
        Assert.Equal(15m, refreshed.CollectedAmount);
    }
}