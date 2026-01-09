using PetAdoptionPlatform.Application.DTOs.Auth;
using PetAdoptionPlatform.Application.Validators.Auth;
using PetAdoptionPlatform.Tests.TestUtilities;
using Xunit;

namespace PetAdoptionPlatform.Tests.UnitTests.Validators.Auth;

public class RegisterRequestValidatorTests
{
    private static RegisterRequestDto Valid()
    {
        return new RegisterRequestDto
        {
            Email = "user@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            FirstName = "A",
            LastName = "B",
            City = "Izmir",
            PhoneNumber = "5555555555",
            IsShelter = false
        };
    }

    [Fact]
    public void Validate_ShouldFail_WhenEmailInvalid()
    {
        var v = new RegisterRequestValidator();
        var dto = Valid();
        dto.Email = "not-an-email";

        var result = v.Validate(dto);

        Assert.False(result.IsValid);
        ValidationAssert.HasErrorFor(result, "Email");
    }

    [Fact]
    public void Validate_ShouldFail_WhenPasswordTooShort()
    {
        var v = new RegisterRequestValidator();
        var dto = Valid();
        dto.Password = "short";
        dto.ConfirmPassword = "short";

        var result = v.Validate(dto);

        Assert.False(result.IsValid);
        ValidationAssert.HasErrorFor(result, "Password");
    }

    [Fact]
    public void Validate_ShouldFail_WhenConfirmPasswordMismatch()
    {
        var v = new RegisterRequestValidator();
        var dto = Valid();
        dto.ConfirmPassword = "DIFFERENT123!";

        var result = v.Validate(dto);

        Assert.False(result.IsValid);
        ValidationAssert.HasErrorFor(result, "ConfirmPassword");
    }

    [Fact]
    public void Validate_ShouldFail_WhenFirstNameEmpty()
    {
        var v = new RegisterRequestValidator();
        var dto = Valid();
        dto.FirstName = "";

        var result = v.Validate(dto);

        Assert.False(result.IsValid);
        ValidationAssert.HasErrorFor(result, "FirstName");
    }

    [Fact]
    public void Validate_ShouldPass_WhenValid()
    {
        var v = new RegisterRequestValidator();
        var result = v.Validate(Valid());

        Assert.True(result.IsValid);
    }
}
