using Microsoft.Extensions.Options;

namespace backend.Modules.AiCoaching.Configuration;

public sealed class AiCoachInteractionOptionsValidator : IValidateOptions<AiCoachInteractionOptions>
{
    public ValidateOptionsResult Validate(string? name, AiCoachInteractionOptions options)
    {
        var errors = new List<string>();

        ValidateRange(options.DailyQuestionLimit, 1, 100, "AiCoachInteraction:DailyQuestionLimit", errors);
        ValidateRange(options.ConversationContextMessageLimit, 2, 20,
            "AiCoachInteraction:ConversationContextMessageLimit", errors);
        ValidateRange(options.ConversationSnapshotLookbackDays, 7, 365,
            "AiCoachInteraction:ConversationSnapshotLookbackDays", errors);
        ValidateRange(options.NutritionSnapshotLookbackDays, 7, 365,
            "AiCoachInteraction:NutritionSnapshotLookbackDays", errors);
        ValidateRange(options.DailySnapshotLookbackDays, 1, 30,
            "AiCoachInteraction:DailySnapshotLookbackDays", errors);
        ValidateRange(options.WorkerPollSeconds, 1, 60, "AiCoachInteraction:WorkerPollSeconds", errors);
        ValidateRange(options.ProcessingLeaseSeconds, 60, 600,
            "AiCoachInteraction:ProcessingLeaseSeconds", errors);

        return errors.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(errors);
    }

    private static void ValidateRange(int value, int minimum, int maximum, string name, ICollection<string> errors)
    {
        if (value < minimum || value > maximum)
            errors.Add($"{name} must be between {minimum} and {maximum}.");
    }
}
