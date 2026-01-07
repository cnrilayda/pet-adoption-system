using FluentValidation;
using PetAdoptionPlatform.Application.DTOs.Ratings;

namespace PetAdoptionPlatform.Application.Validators.Ratings;

public class CreateRatingValidator : AbstractValidator<CreateRatingDto>
{
    public CreateRatingValidator()
    {
        RuleFor(x => x.ApplicationId)
            .NotEmpty().WithMessage("ApplicationId is required");

        RuleFor(x => x.Score)
            .InclusiveBetween(1, 5).WithMessage("Score must be between 1 and 5");

        RuleFor(x => x.Comment)
            .MaximumLength(1000).WithMessage("Comment must not exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Comment));
    }
}

