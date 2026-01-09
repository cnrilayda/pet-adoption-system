using PetAdoptionPlatform.Application.DTOs.Listings;
using PetAdoptionPlatform.Application.Validators.Listings;
using PetAdoptionPlatform.Tests.TestUtilities;
using Xunit;

namespace PetAdoptionPlatform.Tests.UnitTests.Validators.Listings;

public class UpdatePetListingValidatorTests
{
    [Fact]
    public void Validate_ShouldFail_WhenTitleTooLong()
    {
        var v = new UpdatePetListingValidator();
        var dto = new UpdatePetListingDto { Title = new string('a', 201) };

        var result = v.Validate(dto);

        Assert.False(result.IsValid);
        ValidationAssert.HasErrorFor(result, "Title");
    }

    [Fact]
    public void Validate_ShouldFail_WhenRequiredAmountNonPositive()
    {
        var v = new UpdatePetListingValidator();
        var dto = new UpdatePetListingDto { RequiredAmount = 0m };

        var result = v.Validate(dto);

        Assert.False(result.IsValid);
        ValidationAssert.HasErrorFor(result, "RequiredAmount");
    }

    [Fact]
    public void Validate_ShouldPass_WhenNoFieldsProvided()
    {
        var v = new UpdatePetListingValidator();
        var dto = new UpdatePetListingDto();

        var result = v.Validate(dto);

        Assert.True(result.IsValid);
    }
}
