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
        
        RuleFor(x => x.TimeZoneId)
            .Must(BeKnownTimeZone)
            .When(x => !string.IsNullOrWhiteSpace(x.TimeZoneId))
            .WithMessage("TimeZoneId must be a valid system timezone identifier.");
    }

    private static bool BeKnownTimeZone(string? timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
            return false;

        try
        {
            _ = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return true;
        }
        catch (TimeZoneNotFoundException)
        {
            return false;
        }
        catch (InvalidTimeZoneException)
        {
            return false;
        }
    }
}
