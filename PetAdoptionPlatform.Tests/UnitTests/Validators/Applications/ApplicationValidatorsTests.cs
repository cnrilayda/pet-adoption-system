using PetAdoptionPlatform.Application.DTOs.Applications;
using PetAdoptionPlatform.Application.Validators.Applications;
using PetAdoptionPlatform.Domain.Enums;
using PetAdoptionPlatform.Tests.TestUtilities;
using Xunit;

namespace PetAdoptionPlatform.Tests.UnitTests.Validators.Applications;

public class ApplicationValidatorsTests
{
    [Fact]
    public void CreateApplication_ShouldFail_WhenListingIdEmpty()
    {
        var v = new CreateApplicationValidator();
        var result = v.Validate(new CreateApplicationDto { ListingId = default, Message = "x" });

        Assert.False(result.IsValid);
        ValidationAssert.HasErrorFor(result, "ListingId");
    }

    [Fact]
    public void CreateApplication_ShouldFail_WhenMessageTooLong()
    {
        var v = new CreateApplicationValidator();
        var result = v.Validate(new CreateApplicationDto { ListingId = System.Guid.NewGuid(), Message = new string('a', 1001) });

        Assert.False(result.IsValid);
        ValidationAssert.HasErrorFor(result, "Message");
    }

    [Fact]
    public void UpdateStatus_ShouldFail_WhenAdminNotesTooLong()
    {
        var v = new UpdateApplicationStatusValidator();
        var result = v.Validate(new UpdateApplicationStatusDto { Status = ApplicationStatus.Pending, AdminNotes = new string('a', 2001) });

        Assert.False(result.IsValid);
        ValidationAssert.HasErrorFor(result, "AdminNotes");
    }

    [Fact]
    public void UpdateStatus_ShouldPass_WhenValid()
    {
        var v = new UpdateApplicationStatusValidator();
        var result = v.Validate(new UpdateApplicationStatusDto { Status = ApplicationStatus.Accepted });

        Assert.True(result.IsValid);
    }
}
