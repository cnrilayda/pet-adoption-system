using FluentValidation;
using PetAdoptionPlatform.Application.DTOs.Stories;

namespace PetAdoptionPlatform.Application.Validators.Stories;

public class CreateStoryValidator : AbstractValidator<CreateStoryDto>
{
    public CreateStoryValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required")
            .MaximumLength(5000).WithMessage("Content must not exceed 5000 characters");

        RuleFor(x => x.PhotoUrl)
            .MaximumLength(500).WithMessage("Photo URL must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.PhotoUrl));
    }
}

