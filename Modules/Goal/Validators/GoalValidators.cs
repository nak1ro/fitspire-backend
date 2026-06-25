using backend.Modules.Goal.DTOs;
using FluentValidation;

namespace backend.Modules.Goal.Validators;

public class CreateGoalRequestValidator : AbstractValidator<CreateGoalRequest>
{
    private static readonly HashSet<string> AllowedRecurrencePatterns = new(StringComparer.OrdinalIgnoreCase)
    {
        "daily",
        "weekly",
        "monthly"
    };

    public CreateGoalRequestValidator()
    {
        RuleFor(x => x.GoalTypeId)
            .NotEmpty();

        RuleFor(x => x.TargetValue)
            .GreaterThan(0);

        RuleFor(x => x.Unit)
            .NotEmpty()
            .MaximumLength(32);

        RuleFor(x => x.Deadline)
            .GreaterThan(DateTime.UtcNow)
            .When(x => x.Deadline.HasValue)
            .WithMessage("Deadline must be in the future.");

        RuleFor(x => x.RecurrencePattern)
            .NotEmpty()
            .When(x => x.IsRecurring)
            .WithMessage("Recurrence pattern is required for recurring goals.");

        RuleFor(x => x.RecurrencePattern)
            .Must(pattern => pattern is null || AllowedRecurrencePatterns.Contains(pattern))
            .WithMessage("Recurrence pattern must be daily, weekly, or monthly.");

        RuleFor(x => x.SelectedWorkoutType)
            .Must(type => type is null or "gym" or "running" or "cycling" or "swimming" or "yoga")
            .WithMessage("Selected workout type is not supported.");
    }
}

public class UpdateGoalRequestValidator : AbstractValidator<UpdateGoalRequest>
{
    public UpdateGoalRequestValidator()
    {
        RuleFor(request => request.TargetValue).GreaterThan(0);
    }
}
