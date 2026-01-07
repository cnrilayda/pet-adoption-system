using FluentValidation;
using PetAdoptionPlatform.Application.DTOs.Donations;

namespace PetAdoptionPlatform.Application.Validators.Donations;

public class CreateDonationValidator : AbstractValidator<CreateDonationDto>
{
    public CreateDonationValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Donation amount must be greater than 0")
            .LessThanOrEqualTo(100000).WithMessage("Donation amount must be reasonable (max 100,000)");

        RuleFor(x => x.Message)
            .MaximumLength(500).WithMessage("Message must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Message));
    }
}

