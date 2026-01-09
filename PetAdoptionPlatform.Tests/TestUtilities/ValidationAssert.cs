using System.Linq;
using FluentValidation.Results;
using Xunit;

namespace PetAdoptionPlatform.Tests.TestUtilities;

public static class ValidationAssert
{
    public static void HasErrorFor(ValidationResult result, string propertyName)
    {
        Assert.Contains(result.Errors, e => e.PropertyName == propertyName);
    }

    public static void NoErrorFor(ValidationResult result, string propertyName)
    {
        Assert.DoesNotContain(result.Errors, e => e.PropertyName == propertyName);
    }

    public static string[] ErrorMessagesFor(ValidationResult result, string propertyName)
    {
        return result.Errors.Where(e => e.PropertyName == propertyName).Select(e => e.ErrorMessage).ToArray();
    }
}
