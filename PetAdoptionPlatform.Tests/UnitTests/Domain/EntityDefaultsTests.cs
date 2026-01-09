using PetAdoptionPlatform.Domain.Entities;
using PetAdoptionPlatform.Domain.Enums;
using Xunit;

namespace PetAdoptionPlatform.Tests.UnitTests.Domain;

public class EntityDefaultsTests
{
    [Fact]
    public void User_Defaults_AreExpected()
    {
        var u = new User();

        Assert.True(u.IsActive);
        Assert.False(u.IsAdmin);
        Assert.False(u.IsShelter);
        Assert.False(u.IsDeleted);
    }

    [Fact]
    public void PetListing_Defaults_AreExpected()
    {
        var l = new PetListing();

        Assert.True(l.IsActive);
        Assert.False(l.IsApproved);
        Assert.False(l.IsPaused);
        Assert.False(l.IsDeleted);
        // ListingType has no default initializer in entity; default(enum) == 0 until explicitly set via DTO/Service.
        Assert.Equal(default(ListingType), l.Type);
    }

    [Fact]
    public void Complaint_DefaultStatus_IsOpen()
    {
        var c = new Complaint();
        Assert.Equal(ComplaintStatus.Open, c.Status);
    }
}