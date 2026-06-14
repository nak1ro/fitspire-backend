using backend.Modules.User.DTOs;
using FluentValidation;

namespace backend.Modules.User.Validators;

public class UpdateProfileDtoValidator : AbstractValidator<UpdateProfileDto>
{
    public UpdateProfileDtoValidator()
    {
        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Display name is required.")
            .MaximumLength(30).WithMessage("Display name must be at most 30 characters.")
            .When(x => x.DisplayName is not null);

        RuleFor(x => x.Bio)
            .MaximumLength(300).WithMessage("Bio must be at most 300 characters.")
            .When(x => x.Bio is not null);

        RuleFor(x => x)
            .Must(x => x.DisplayName is not null || x.Bio is not null || x.IsPrivate.HasValue)
            .WithMessage("At least one profile field must be provided.");
    }
}
