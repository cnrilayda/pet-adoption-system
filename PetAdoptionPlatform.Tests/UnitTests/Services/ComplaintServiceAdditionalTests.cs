using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PetAdoptionPlatform.Application.DTOs.Complaints;
using PetAdoptionPlatform.Domain.Entities;
using PetAdoptionPlatform.Domain.Enums;
using PetAdoptionPlatform.Infrastructure.Services;
using PetAdoptionPlatform.Tests.TestUtilities;
using Xunit;

namespace PetAdoptionPlatform.Tests.UnitTests.Services;

public class ComplaintServiceAdditionalTests
{
    [Fact]
    public async Task CreateComplaintAsync_ShouldThrow_WhenComplainantInactive()
    {
        using var ctx = TestDbContextFactory.Create();
        var complainant = SeedHelper.CreateUser("c@x.com", isActive: false);
        var target = SeedHelper.CreateUser("t@x.com");
        await SeedHelper.SaveAsync(ctx, complainant, target);

        var sut = new ComplaintService(ctx);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            sut.CreateComplaintAsync(new CreateComplaintDto { TargetUserId = target.Id, Reason = "R", Description = "D" }, complainant.Id));
    }

    [Fact]
    public async Task CreateComplaintAsync_ShouldThrow_WhenTargetUserNotFound()
    {
        using var ctx = TestDbContextFactory.Create();
        var complainant = SeedHelper.CreateUser("c@x.com");
        await SeedHelper.SaveAsync(ctx, complainant);

        var sut = new ComplaintService(ctx);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            sut.CreateComplaintAsync(new CreateComplaintDto { TargetUserId = Guid.NewGuid(), Reason = "R", Description = "D" }, complainant.Id));
    }

    [Fact]
    public async Task CreateComplaintAsync_ShouldSetStatusOpen()
    {
        using var ctx = TestDbContextFactory.Create();
        var complainant = SeedHelper.CreateUser("c@x.com");
        var target = SeedHelper.CreateUser("t@x.com");
        await SeedHelper.SaveAsync(ctx, complainant, target);

        var sut = new ComplaintService(ctx);

        var dto = await sut.CreateComplaintAsync(new CreateComplaintDto { TargetUserId = target.Id, Reason = "R", Description = "D" }, complainant.Id);

        Assert.Equal(ComplaintStatus.Open, dto.Status);
        Assert.Equal(target.Id, dto.TargetUserId);
    }

    [Fact]
    public async Task GetMyComplaintsAsync_ShouldReturnOnlyMyComplaints()
    {
        using var ctx = TestDbContextFactory.Create();
        var u1 = SeedHelper.CreateUser("u1@x.com");
        var u2 = SeedHelper.CreateUser("u2@x.com");
        var c1 = new Complaint { ComplainantId = u1.Id, Reason = "R", Description = "D", Status = ComplaintStatus.Open };
        var c2 = new Complaint { ComplainantId = u2.Id, Reason = "R2", Description = "D2", Status = ComplaintStatus.Open };
        await SeedHelper.SaveAsync(ctx, u1, u2, c1, c2);

        var sut = new ComplaintService(ctx);

        var list = (await sut.GetMyComplaintsAsync(u1.Id)).ToList();

        Assert.Single(list);
        Assert.Equal(u1.Id, list[0].ComplainantId);
    }

    [Fact]
    public async Task UpdateComplaintStatusAsync_ShouldUpdateFields()
    {
        using var ctx = TestDbContextFactory.Create();

        // UpdateComplaintStatusAsync resolves "admin" internally (first user where IsAdmin==true).
        var admin = SeedHelper.CreateUser("admin@x.com", isAdmin: true);

        var u = SeedHelper.CreateUser("u@x.com");
        var complaint = new Complaint { ComplainantId = u.Id, Reason = "R", Description = "D", Status = ComplaintStatus.Open };

        await SeedHelper.SaveAsync(ctx, admin, u, complaint);

        var sut = new ComplaintService(ctx);

        var updated = await sut.UpdateComplaintStatusAsync(complaint.Id, new UpdateComplaintStatusDto
        {
            Status = ComplaintStatus.Resolved,
            AdminNotes = "admin",
            ResolutionNotes = "done"
        });

        Assert.Equal(ComplaintStatus.Resolved, updated.Status);
        Assert.Equal("admin", updated.AdminNotes);
        Assert.Equal("done", updated.ResolutionNotes);
    }
}
