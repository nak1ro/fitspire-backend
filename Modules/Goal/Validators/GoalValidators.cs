using backend.Modules.Goal.DTOs;
using backend.Modules.Goal.Domain.Constants;
using backend.Modules.Goal.Domain.Enums;
using FluentValidation;

namespace backend.Modules.Goal.Validators;

public class CreateGoalRequestValidator : AbstractValidator<CreateGoalRequest>
{
    public CreateGoalRequestValidator()
    {
        RuleFor(x => x.GoalTypeId)
            .NotEmpty();

        RuleFor(x => x.TargetValue)
            .Must(value => !double.IsNaN(value) && !double.IsInfinity(value))
            .GreaterThan(0)
            .LessThanOrEqualTo(1_000_000_000);

        RuleFor(x => x.Schedule)
            .NotEmpty()
            .Must(GoalSchedules.All.Contains)
            .WithMessage("Schedule must be one-off, daily, weekly, or monthly.");

        RuleFor(x => x.Deadline)
            .GreaterThan(DateTime.UtcNow)
            .When(x => string.Equals(x.Schedule, GoalSchedules.OneOff, StringComparison.OrdinalIgnoreCase))
            .WithMessage("One-off goals require a future deadline.");

        RuleFor(x => x.Deadline)
            .Null()
            .When(x => !string.Equals(x.Schedule, GoalSchedules.OneOff, StringComparison.OrdinalIgnoreCase))
            .WithMessage("Recurring goals do not use an overall deadline.");

        RuleFor(x => x.StartDate)
            .GreaterThanOrEqualTo(DateTime.UtcNow.AddMinutes(-1))
            .When(x => x.StartDate.HasValue)
            .WithMessage("Goal start date cannot be in the past.");

        RuleFor(x => x.Deadline)
            .GreaterThan(x => x.StartDate!.Value)
            .When(x => x.StartDate.HasValue && x.Deadline.HasValue)
            .WithMessage("Goal deadline must be after its start date.");

        RuleFor(x => x.SelectedWorkoutType)
            .Must(type => type is null or "gym" or "running" or "cycling" or "swimming" or "yoga")
            .WithMessage("Selected workout type is not supported.");
    }
}

public class UpdateGoalRequestValidator : AbstractValidator<UpdateGoalRequest>
{
    public UpdateGoalRequestValidator()
    {
        RuleFor(request => request.TargetValue)
            .Must(value => !double.IsNaN(value) && !double.IsInfinity(value))
            .GreaterThan(0)
            .LessThanOrEqualTo(1_000_000_000);
        RuleFor(request => request.Deadline)
            .GreaterThan(DateTime.UtcNow)
            .When(request => request.Deadline.HasValue)
            .WithMessage("Goal deadline must be in the future.");
    }
}

public class GoalListFilterValidator : AbstractValidator<GoalListFilter>
{
    private static readonly HashSet<string> Scopes = new(StringComparer.OrdinalIgnoreCase) { "active", "history", "all" };

    public GoalListFilterValidator()
    {
        RuleFor(filter => filter.Scope).Must(Scopes.Contains).WithMessage("Scope must be active, history, or all.");
        RuleFor(filter => filter.Status)
            .Must(status => string.IsNullOrWhiteSpace(status) || Enum.TryParse<GoalStatus>(status, true, out _))
            .WithMessage("Status is not supported.");
        RuleFor(filter => filter.Page).GreaterThan(0);
        RuleFor(filter => filter.PageSize).InclusiveBetween(1, 100);
    }
}

public class GoalPaginationValidator : AbstractValidator<GoalPagination>
{
    public GoalPaginationValidator()
    {
        RuleFor(filter => filter.Page).GreaterThan(0);
        RuleFor(filter => filter.PageSize).InclusiveBetween(1, 100);
    }
}
