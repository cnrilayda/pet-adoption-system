using FluentValidation;
using PetAdoptionPlatform.Application.DTOs.Applications;

namespace PetAdoptionPlatform.Application.Validators.Applications;

public class CreateApplicationValidator : AbstractValidator<CreateApplicationDto>
{
    public CreateApplicationValidator()
    {
        RuleFor(x => x.ListingId)
            .NotEmpty().WithMessage("Listing ID is required");

        RuleFor(x => x.Message)
            .MaximumLength(1000).WithMessage("Message must not exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Message));
    }
}

