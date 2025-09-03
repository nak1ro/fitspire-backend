using backend.Modules.User.DTOs;
using FluentValidation;

namespace backend.Modules.User.Validators;

public class UpdateUserPreferencesDtoValidator : AbstractValidator<UpdateUserPreferencesDto>
{
    public UpdateUserPreferencesDtoValidator()
    {
        RuleFor(x => x.PreferredLanguage)
            .Must(lang => lang == null || new[] { "en", "es", "ru" }.Contains(lang))
            .WithMessage("PreferredLanguage must be one of: en, es, ru.");

        RuleFor(x => x.UnitSystem)
            .Must(unit => unit is null or "metric" or "imperial")
            .WithMessage("UnitSystem must be 'metric' or 'imperial'.");
        
        RuleFor(x => x.UnitSystem)
            .Must(x => x is null or "metric" or "imperial")
            .WithMessage("Unit system must be 'metric' or 'imperial'.");
    }
}