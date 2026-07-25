using FluentValidation;

namespace backend.Modules.AiCoaching.Contracts;

public sealed class GenerateWeeklyCoachReportRequestValidator : AbstractValidator<GenerateWeeklyCoachReportRequest>
{
    public GenerateWeeklyCoachReportRequestValidator()
    {
        RuleFor(request => request.PeriodStart)
            .Must(date => !date.HasValue || (date.Value != DateOnly.MinValue && date.Value.DayOfWeek == DayOfWeek.Monday))
            .WithMessage("PeriodStart must be a Monday.");
    }
}

public sealed class WeeklyCoachReportHistoryFilterValidator : AbstractValidator<WeeklyCoachReportHistoryFilter>
{
    public WeeklyCoachReportHistoryFilterValidator()
    {
        RuleFor(filter => filter.Page).InclusiveBetween(1, 10_000);
        RuleFor(filter => filter.PageSize).InclusiveBetween(1, 50);
    }
}
