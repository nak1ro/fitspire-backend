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

        RuleFor(x => x.FavoriteSport).IsInEnum().When(x => x.FavoriteSport.HasValue);
        RuleFor(x => x.FitnessLevel).IsInEnum().When(x => x.FitnessLevel.HasValue);
        RuleFor(x => x.HeightCm)
            .InclusiveBetween(50, 260).WithMessage("Height must be between 50 and 260 cm.")
            .When(x => x.HeightCm.HasValue);

        RuleFor(x => x)
            .Must(x => x.DisplayName is not null || x.Bio is not null || x.IsPrivate.HasValue ||
                       x.FavoriteSport.HasValue || x.FitnessLevel.HasValue || x.HeightCm.HasValue)
            .WithMessage("At least one profile field must be provided.");
    }
}
