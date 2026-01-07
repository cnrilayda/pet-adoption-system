using FluentValidation;
using PetAdoptionPlatform.Application.DTOs.Applications;

namespace PetAdoptionPlatform.Application.Validators.Applications;

public class UpdateApplicationStatusValidator : AbstractValidator<UpdateApplicationStatusDto>
{
    public UpdateApplicationStatusValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid application status");

        RuleFor(x => x.AdminNotes)
            .MaximumLength(2000).WithMessage("Admin notes must not exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.AdminNotes));
    }
}

