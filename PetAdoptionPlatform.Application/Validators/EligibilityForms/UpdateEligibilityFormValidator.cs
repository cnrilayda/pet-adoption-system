using FluentValidation;
using PetAdoptionPlatform.Application.DTOs.EligibilityForms;

namespace PetAdoptionPlatform.Application.Validators.EligibilityForms;

public class UpdateEligibilityFormValidator : AbstractValidator<UpdateEligibilityFormDto>
{
    public UpdateEligibilityFormValidator()
    {
        RuleFor(x => x.LivingType)
            .MaximumLength(50).WithMessage("Living type must not exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.LivingType));

        RuleFor(x => x.SquareMeters)
            .GreaterThan(0).WithMessage("Square meters must be greater than 0")
            .LessThanOrEqualTo(10000).WithMessage("Square meters must be reasonable")
            .When(x => x.SquareMeters.HasValue);

        RuleFor(x => x.PreviousPetTypes)
            .MaximumLength(200).WithMessage("Previous pet types must not exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.PreviousPetTypes));

        RuleFor(x => x.YearsOfExperience)
            .GreaterThanOrEqualTo(0).WithMessage("Years of experience must be 0 or greater")
            .LessThanOrEqualTo(100).WithMessage("Years of experience must be reasonable")
            .When(x => x.YearsOfExperience.HasValue);

        RuleFor(x => x.HouseholdMembers)
            .GreaterThan(0).WithMessage("Household members must be greater than 0")
            .LessThanOrEqualTo(50).WithMessage("Household members must be reasonable")
            .When(x => x.HouseholdMembers.HasValue);

        RuleFor(x => x.ChildrenAge)
            .GreaterThanOrEqualTo(0).WithMessage("Children age must be 0 or greater")
            .LessThanOrEqualTo(18).WithMessage("Children age must be reasonable")
            .When(x => x.ChildrenAge.HasValue);

        RuleFor(x => x.WorkSchedule)
            .MaximumLength(50).WithMessage("Work schedule must not exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.WorkSchedule));

        RuleFor(x => x.HoursAwayFromHome)
            .GreaterThanOrEqualTo(0).WithMessage("Hours away from home must be 0 or greater")
            .LessThanOrEqualTo(24).WithMessage("Hours away from home must be reasonable")
            .When(x => x.HoursAwayFromHome.HasValue);

        RuleFor(x => x.MonthlyBudgetForPet)
            .GreaterThan(0).WithMessage("Monthly budget must be greater than 0")
            .When(x => x.MonthlyBudgetForPet.HasValue);

        RuleFor(x => x.AdditionalNotes)
            .MaximumLength(2000).WithMessage("Additional notes must not exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.AdditionalNotes));
    }
}

