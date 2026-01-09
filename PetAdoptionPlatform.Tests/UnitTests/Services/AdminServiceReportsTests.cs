using System;
using System.Threading.Tasks;
using PetAdoptionPlatform.Application.DTOs.Admin;
using PetAdoptionPlatform.Domain.Entities;
using PetAdoptionPlatform.Domain.Enums;
using PetAdoptionPlatform.Infrastructure.Services;
using PetAdoptionPlatform.Tests.TestUtilities;
using Xunit;

namespace PetAdoptionPlatform.Tests.UnitTests.Services;

public class AdminServiceReportsTests
{
    [Fact]
    public async Task GetReportsAsync_ShouldCountUsersByStatus()
    {
        using var ctx = TestDbContextFactory.Create();

        var active = SeedHelper.CreateUser("a@x.com", isActive: true);
        var banned = SeedHelper.CreateUser("b@x.com", isActive: true);
        banned.IsBanned = true;

        var inactive = SeedHelper.CreateUser("c@x.com", isActive: false);

        var shelter = SeedHelper.CreateUser("s@x.com", isShelter: true);
        shelter.IsShelterVerified = true;

        await SeedHelper.SaveAsync(ctx, active, banned, inactive, shelter);

        var sut = new AdminService(ctx);

        var r = await sut.GetReportsAsync();

        Assert.Equal(4, r.TotalUsers);
        Assert.Equal(2, r.ActiveUsers); // AdminService counts active AND not banned
        Assert.Equal(1, r.BannedUsers);
        Assert.Equal(1, r.ShelterUsers);
        Assert.Equal(1, r.VerifiedShelters);
    }

    [Fact]
    public async Task GetReportsAsync_ShouldCountListingsByTypeAndApproval()
    {
        using var ctx = TestDbContextFactory.Create();
        var owner = SeedHelper.CreateUser("owner@x.com");
        var pending = SeedHelper.CreateListing(owner.Id, ListingType.Adoption, isApproved: false);
        var adopt = SeedHelper.CreateListing(owner.Id, ListingType.Adoption, isApproved: true);
        var lost = SeedHelper.CreateListing(owner.Id, ListingType.Lost, isApproved: true);
        var help = SeedHelper.CreateListing(owner.Id, ListingType.HelpRequest, isApproved: true);

        await SeedHelper.SaveAsync(ctx, owner, pending, adopt, lost, help);

        var sut = new AdminService(ctx);

        var r = await sut.GetReportsAsync();

        Assert.Equal(4, r.TotalListings);
        Assert.Equal(3, r.ActiveListings);
        Assert.Equal(1, r.PendingApprovalListings);
        Assert.Equal(2, r.AdoptionListings);
        Assert.Equal(1, r.LostListings);
        Assert.Equal(1, r.HelpRequestListings);
    }

    [Fact]
    public async Task GetReportsAsync_ShouldCountApplicationsByStatus()
    {
        using var ctx = TestDbContextFactory.Create();
        var owner = SeedHelper.CreateUser("owner@x.com");
        var adopter = SeedHelper.CreateUser("adopter@x.com");
        var listing = SeedHelper.CreateListing(owner.Id, ListingType.Adoption, isApproved: true);
        var p = SeedHelper.CreateApplication(listing.Id, adopter.Id, ApplicationStatus.Pending);
        var a = SeedHelper.CreateApplication(listing.Id, adopter.Id, ApplicationStatus.Accepted);
        var c = SeedHelper.CreateApplication(listing.Id, adopter.Id, ApplicationStatus.Completed);

        await SeedHelper.SaveAsync(ctx, owner, adopter, listing, p, a, c);

        var sut = new AdminService(ctx);

        var r = await sut.GetReportsAsync();

        Assert.Equal(3, r.TotalApplications);
        Assert.Equal(1, r.PendingApplications);
        Assert.Equal(1, r.AcceptedApplications);
        Assert.Equal(1, r.CompletedAdoptions);
    }

    [Fact]
    public async Task GetReportsAsync_ShouldSumCompletedDonationsOnly()
    {
        using var ctx = TestDbContextFactory.Create();
        var donor = SeedHelper.CreateUser("donor@x.com");
        var d1 = new Donation { DonorId = donor.Id, Amount = 10m, PaymentStatus = "Completed", PaymentTransactionId = "t1" };
        var d2 = new Donation { DonorId = donor.Id, Amount = 20m, PaymentStatus = "Failed", PaymentTransactionId = "t2" };

        await SeedHelper.SaveAsync(ctx, donor, d1, d2);

        var sut = new AdminService(ctx);

        var r = await sut.GetReportsAsync();

        Assert.Equal(1, r.TotalDonations); // AdminService counts only completed
        Assert.Equal(10m, r.TotalDonationAmount);
    }

    [Fact]
    public async Task GetReportsAsync_ShouldCountComplaintsByStatus()
    {
        using var ctx = TestDbContextFactory.Create();
        var u = SeedHelper.CreateUser("u@x.com");
        var open = new Complaint { ComplainantId = u.Id, Reason = "R", Description = "D", Status = ComplaintStatus.Open };
        var resolved = new Complaint { ComplainantId = u.Id, Reason = "R2", Description = "D2", Status = ComplaintStatus.Resolved };

        await SeedHelper.SaveAsync(ctx, u, open, resolved);

        var sut = new AdminService(ctx);

        var r = await sut.GetReportsAsync();

        Assert.Equal(2, r.TotalComplaints);
        Assert.Equal(1, r.OpenComplaints);
        Assert.Equal(1, r.ResolvedComplaints);
    }

    [Fact]
    public async Task GetReportsAsync_ShouldCountStoriesByApproval()
    {
        using var ctx = TestDbContextFactory.Create();
        var u = SeedHelper.CreateUser("u@x.com");
        var s1 = new Story { AuthorId = u.Id, Title = "t1", Content = "c1", Status = StoryStatus.Pending };
        var s2 = new Story { AuthorId = u.Id, Title = "t2", Content = "c2", Status = StoryStatus.Approved };

        await SeedHelper.SaveAsync(ctx, u, s1, s2);

        var sut = new AdminService(ctx);

        var r = await sut.GetReportsAsync();

        Assert.Equal(1, r.PendingStories);
        Assert.Equal(1, r.ApprovedStories);
    }

    [Fact]
    public async Task UpdateUserStatusAsync_ShouldUpdateOnlyProvidedFields()
    {
        using var ctx = TestDbContextFactory.Create();
        var u = SeedHelper.CreateUser("u@x.com", isActive: true);
        u.IsBanned = false;
        u.IsShelterVerified = false;

        await SeedHelper.SaveAsync(ctx, u);

        var sut = new AdminService(ctx);

        await sut.UpdateUserStatusAsync(u.Id, new UpdateUserStatusDto { IsActive = false, IsBanned = true });

        var refreshed = await sut.GetUserByIdAsync(u.Id);

        Assert.False(refreshed.IsActive);
        Assert.True(refreshed.IsBanned);
        Assert.False(refreshed.IsShelterVerified);
    }
}