using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PetAdoptionPlatform.Application.DTOs.Complaints;
using PetAdoptionPlatform.Domain.Enums;
using PetAdoptionPlatform.Infrastructure.Services;
using PetAdoptionPlatform.Tests.TestUtilities;
using Xunit;

namespace PetAdoptionPlatform.Tests.UnitTests.Services;

public class ComplaintServiceTests
{
    [Fact]
    public async Task CreateComplaintAsync_ShouldCreateOpenComplaint()
    {
        using var ctx = TestDbContextFactory.Create();
        var complainant = SeedHelper.CreateUser("c@x.com");
        var target = SeedHelper.CreateUser("t@x.com");
        await SeedHelper.SaveAsync(ctx, complainant, target);

        var sut = new ComplaintService(ctx);

        var created = await sut.CreateComplaintAsync(new CreateComplaintDto
        {
            TargetUserId = target.Id,
            Reason = "Spam"
        }, complainant.Id);

        Assert.Equal(ComplaintStatus.Open, created.Status);
        Assert.Equal("Spam", created.Reason);
        Assert.Equal(complainant.Id, created.ComplainantId);
        Assert.Equal(target.Id, created.TargetUserId);
    }

    [Fact]
    public async Task GetComplaintByIdAsync_ShouldThrowUnauthorized_ForUnrelatedUser()
    {
        using var ctx = TestDbContextFactory.Create();
        var complainant = SeedHelper.CreateUser("c@x.com");
        var target = SeedHelper.CreateUser("t@x.com");
        var other = SeedHelper.CreateUser("o@x.com");
        await SeedHelper.SaveAsync(ctx, complainant, target, other);

        var sut = new ComplaintService(ctx);

        var created = await sut.CreateComplaintAsync(new CreateComplaintDto
        {
            TargetUserId = target.Id,
            Reason = "Spam"
        }, complainant.Id);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            sut.GetComplaintByIdAsync(created.Id, other.Id));
    }

    [Fact]
    public async Task UpdateComplaintStatusAsync_ShouldUpdateStatus_AndNotes()
    {
        using var ctx = TestDbContextFactory.Create();
        var complainant = SeedHelper.CreateUser("c@x.com");
        var target = SeedHelper.CreateUser("t@x.com");
        var admin = SeedHelper.CreateUser("admin@x.com", isAdmin: true);
        await SeedHelper.SaveAsync(ctx, complainant, target, admin);

        var sut = new ComplaintService(ctx);

        var created = await sut.CreateComplaintAsync(new CreateComplaintDto
        {
            TargetUserId = target.Id,
            Reason = "Spam"
        }, complainant.Id);

        var updated = await sut.UpdateComplaintStatusAsync(created.Id, new UpdateComplaintStatusDto
        {
            Status = ComplaintStatus.InReview,
            AdminNotes = "checking",
            ResolutionNotes = "pending"
        });

        Assert.Equal(ComplaintStatus.InReview, updated.Status);
        Assert.Equal("checking", updated.AdminNotes);
        Assert.Equal("pending", updated.ResolutionNotes);
        Assert.NotNull(updated.UpdatedAt);
    }

    [Fact]
    public async Task CreateComplaintAsync_ShouldThrow_WhenTargetListingNotFound()
    {
        using var ctx = TestDbContextFactory.Create();
        var complainant = SeedHelper.CreateUser("c@x.com");
        await SeedHelper.SaveAsync(ctx, complainant);

        var sut = new ComplaintService(ctx);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            sut.CreateComplaintAsync(new CreateComplaintDto
            {
                TargetListingId = Guid.NewGuid(),
                Reason = "bad"
            }, complainant.Id));
    }
}
