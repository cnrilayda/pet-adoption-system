using PetAdoptionPlatform.Application.DTOs.Complaints;
using PetAdoptionPlatform.Application.DTOs.Donations;
using PetAdoptionPlatform.Application.DTOs.EligibilityForms;
using PetAdoptionPlatform.Application.DTOs.Messages;
using PetAdoptionPlatform.Application.DTOs.Ratings;
using PetAdoptionPlatform.Application.DTOs.Stories;
using PetAdoptionPlatform.Application.Validators.Complaints;
using PetAdoptionPlatform.Application.Validators.Donations;
using PetAdoptionPlatform.Application.Validators.EligibilityForms;
using PetAdoptionPlatform.Application.Validators.Messages;
using PetAdoptionPlatform.Application.Validators.Ratings;
using PetAdoptionPlatform.Application.Validators.Stories;
using PetAdoptionPlatform.Tests.TestUtilities;
using Xunit;

namespace PetAdoptionPlatform.Tests.UnitTests.Validators.Misc;

public class MiscValidatorsTests
{
    [Fact]
    public void CreateComplaint_ShouldFail_WhenNoTargetProvided()
    {
        var v = new CreateComplaintValidator();
        var dto = new CreateComplaintDto { Reason = "R", Description = "D" };

        var result = v.Validate(dto);

        Assert.False(result.IsValid);
        // Model-level error has empty PropertyName in FluentValidation
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("Either TargetUserId"));
    }

    [Fact]
    public void CreateComplaint_ShouldFail_WhenReasonEmpty()
    {
        var v = new CreateComplaintValidator();
        var dto = new CreateComplaintDto { TargetUserId = System.Guid.NewGuid(), Reason = "", Description = "D" };

        var result = v.Validate(dto);

        Assert.False(result.IsValid);
        ValidationAssert.HasErrorFor(result, "Reason");
    }

    [Fact]
    public void CreateDonation_ShouldFail_WhenAmountNonPositive()
    {
        var v = new CreateDonationValidator();
        var dto = new CreateDonationDto { Amount = 0m };

        var result = v.Validate(dto);

        Assert.False(result.IsValid);
        ValidationAssert.HasErrorFor(result, "Amount");
    }

    [Fact]
    public void CreateDonation_ShouldFail_WhenMessageTooLong()
    {
        var v = new CreateDonationValidator();
        var dto = new CreateDonationDto { Amount = 10m, Message = new string('a', 501) };

        var result = v.Validate(dto);

        Assert.False(result.IsValid);
        ValidationAssert.HasErrorFor(result, "Message");
    }

    [Fact]
    public void CreateEligibilityForm_ShouldFail_WhenHoursAwayTooHigh()
    {
        var v = new CreateEligibilityFormValidator();
        var dto = new CreateEligibilityFormDto { HoursAwayFromHome = 25 };

        var result = v.Validate(dto);

        Assert.False(result.IsValid);
        ValidationAssert.HasErrorFor(result, "HoursAwayFromHome");
    }

    [Fact]
    public void UpdateEligibilityForm_ShouldFail_WhenMonthlyBudgetNonPositive()
    {
        var v = new UpdateEligibilityFormValidator();
        var dto = new UpdateEligibilityFormDto { MonthlyBudgetForPet = 0m };

        var result = v.Validate(dto);

        Assert.False(result.IsValid);
        ValidationAssert.HasErrorFor(result, "MonthlyBudgetForPet");
    }

    [Fact]
    public void CreateMessage_ShouldFail_WhenContentEmpty()
    {
        var v = new CreateMessageValidator();
        var dto = new CreateMessageDto { ApplicationId = System.Guid.NewGuid(), Content = "" };

        var result = v.Validate(dto);

        Assert.False(result.IsValid);
        ValidationAssert.HasErrorFor(result, "Content");
    }

    [Fact]
    public void CreateRating_ShouldFail_WhenScoreOutOfRange()
    {
        var v = new CreateRatingValidator();
        var dto = new CreateRatingDto { ApplicationId = System.Guid.NewGuid(), Score = 6 };

        var result = v.Validate(dto);

        Assert.False(result.IsValid);
        ValidationAssert.HasErrorFor(result, "Score");
    }

    [Fact]
    public void CreateStory_ShouldFail_WhenTitleEmpty()
    {
        var v = new CreateStoryValidator();
        var dto = new CreateStoryDto { Title = "", Content = "content" };

        var result = v.Validate(dto);

        Assert.False(result.IsValid);
        ValidationAssert.HasErrorFor(result, "Title");
    }

    [Fact]
    public void CreateStory_ShouldPass_WhenValid()
    {
        var v = new CreateStoryValidator();
        var dto = new CreateStoryDto { Title = "t", Content = "c" };

        var result = v.Validate(dto);

        Assert.True(result.IsValid);
    }
}
