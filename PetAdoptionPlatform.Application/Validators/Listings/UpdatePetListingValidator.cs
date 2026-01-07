using FluentValidation;
using PetAdoptionPlatform.Application.DTOs.Listings;

namespace PetAdoptionPlatform.Application.Validators.Listings;

public class UpdatePetListingValidator : AbstractValidator<UpdatePetListingDto>
{
    public UpdatePetListingValidator()
    {
        RuleFor(x => x.Title)
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.Title));

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Species)
            .MaximumLength(100).WithMessage("Species must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Species));

        RuleFor(x => x.Breed)
            .MaximumLength(100).WithMessage("Breed must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Breed));

        RuleFor(x => x.Age)
            .GreaterThanOrEqualTo(0).WithMessage("Age must be 0 or greater")
            .LessThanOrEqualTo(300).WithMessage("Age must be reasonable (max 300 months)")
            .When(x => x.Age.HasValue);

        RuleFor(x => x.Gender)
            .Must(g => string.IsNullOrEmpty(g) || g == "Male" || g == "Female" || g == "Unknown")
            .WithMessage("Gender must be Male, Female, or Unknown")
            .When(x => !string.IsNullOrEmpty(x.Gender));

        RuleFor(x => x.Size)
            .Must(s => string.IsNullOrEmpty(s) || s == "Small" || s == "Medium" || s == "Large")
            .WithMessage("Size must be Small, Medium, or Large")
            .When(x => !string.IsNullOrEmpty(x.Size));

        RuleFor(x => x.City)
            .MaximumLength(100).WithMessage("City must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.City));

        RuleFor(x => x.District)
            .MaximumLength(100).WithMessage("District must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.District));

        RuleFor(x => x.RequiredAmount)
            .GreaterThan(0).WithMessage("Required amount must be greater than 0")
            .When(x => x.RequiredAmount.HasValue);
    }
}

