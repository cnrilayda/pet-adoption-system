using FluentValidation;
using PetAdoptionPlatform.Application.DTOs.Complaints;

namespace PetAdoptionPlatform.Application.Validators.Complaints;

public class CreateComplaintValidator : AbstractValidator<CreateComplaintDto>
{
    public CreateComplaintValidator()
    {
        RuleFor(x => x)
            .Must(x => x.TargetUserId.HasValue || x.TargetListingId.HasValue)
            .WithMessage("Either TargetUserId or TargetListingId must be provided.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required")
            .MaximumLength(200).WithMessage("Reason must not exceed 200 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters");
    }
}

