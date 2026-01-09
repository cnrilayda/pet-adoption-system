using PetAdoptionPlatform.Application.DTOs.Auth;
using PetAdoptionPlatform.Application.Validators.Auth;
using PetAdoptionPlatform.Tests.TestUtilities;
using Xunit;

namespace PetAdoptionPlatform.Tests.UnitTests.Validators.Auth;

public class LoginRequestValidatorTests
{
    [Fact]
    public void Validate_ShouldFail_WhenEmailEmpty()
    {
        var v = new LoginRequestValidator();
        var result = v.Validate(new LoginRequestDto { Email = "", Password = "pass" });

        Assert.False(result.IsValid);
        ValidationAssert.HasErrorFor(result, "Email");
    }

    [Fact]
    public void Validate_ShouldFail_WhenPasswordEmpty()
    {
        var v = new LoginRequestValidator();
        var result = v.Validate(new LoginRequestDto { Email = "a@b.com", Password = "" });

        Assert.False(result.IsValid);
        ValidationAssert.HasErrorFor(result, "Password");
    }

    [Fact]
    public void Validate_ShouldPass_WhenValid()
    {
        var v = new LoginRequestValidator();
        var result = v.Validate(new LoginRequestDto { Email = "a@b.com", Password = "pass1234" });

        Assert.True(result.IsValid);
    }
}
