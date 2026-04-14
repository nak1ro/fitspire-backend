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
    }
}

public class UpdateGoalProgressRequestValidator : AbstractValidator<UpdateGoalProgressRequest>
{
    public UpdateGoalProgressRequestValidator()
    {
        RuleFor(x => x.Delta)
            .GreaterThan(0);

        RuleFor(x => x.Source)
            .NotEmpty()
            .When(x => x.Source is not null);

        RuleFor(x => x.Source)
            .MaximumLength(64)
            .When(x => x.Source is not null);
    }
}
