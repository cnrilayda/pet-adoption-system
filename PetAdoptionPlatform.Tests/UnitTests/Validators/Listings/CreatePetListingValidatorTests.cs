using System.Collections.Generic;
using PetAdoptionPlatform.Application.DTOs.Listings;
using PetAdoptionPlatform.Application.Validators.Listings;
using PetAdoptionPlatform.Domain.Enums;
using PetAdoptionPlatform.Tests.TestUtilities;
using Xunit;

namespace PetAdoptionPlatform.Tests.UnitTests.Validators.Listings;

public class CreatePetListingValidatorTests
{
    private static CreatePetListingDto ValidAdoption()
    {
        return new CreatePetListingDto
        {
            Type = ListingType.Adoption,
            Title = "A title",
            Description = "A description",
            City = "Ankara"
        };
    }

    [Fact]
    public void Validate_ShouldFail_WhenTitleEmpty()
    {
        var v = new CreatePetListingValidator();
        var dto = ValidAdoption();
        dto.Title = "";

        var result = v.Validate(dto);

        Assert.False(result.IsValid);
        ValidationAssert.HasErrorFor(result, "Title");
    }

    [Fact]
    public void Validate_ShouldFail_WhenDescriptionEmpty()
    {
        var v = new CreatePetListingValidator();
        var dto = ValidAdoption();
        dto.Description = "";

        var result = v.Validate(dto);

        Assert.False(result.IsValid);
        ValidationAssert.HasErrorFor(result, "Description");
    }

    [Fact]
    public void Validate_ShouldFail_WhenPhotoCountAbove10()
    {
        var v = new CreatePetListingValidator();
        var dto = ValidAdoption();
        dto.PhotoUrls = new List<string>();
        for (var i = 0; i < 11; i++) dto.PhotoUrls.Add($"http://x/{i}.jpg");

        var result = v.Validate(dto);

        Assert.False(result.IsValid);
        ValidationAssert.HasErrorFor(result, "PhotoUrls");
    }

    [Fact]
    public void Validate_ShouldFail_WhenHelpRequestRequiredAmountIsNonPositive()
    {
        var v = new CreatePetListingValidator();
        var dto = ValidAdoption();
        dto.Type = ListingType.HelpRequest;
        dto.RequiredAmount = 0m;

        var result = v.Validate(dto);

        Assert.False(result.IsValid);
        ValidationAssert.HasErrorFor(result, "RequiredAmount");
    }

    [Fact]
    public void Validate_ShouldPass_WhenHelpRequestRequiredAmountPositive()
    {
        var v = new CreatePetListingValidator();
        var dto = ValidAdoption();
        dto.Type = ListingType.HelpRequest;
        dto.RequiredAmount = 100m;

        var result = v.Validate(dto);

        Assert.True(result.IsValid);
    }
}
