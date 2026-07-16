using backend.Modules.BodyTracking.Contracts;
using backend.Modules.BodyTracking.Domain.Constants;
using FluentValidation;

namespace backend.Modules.BodyTracking.Validators;

public class CreateBodyCheckInRequestValidator : BodyCheckInInputValidator<CreateBodyCheckInRequest>
{
    public CreateBodyCheckInRequestValidator()
    {
        RuleFor(request => request.CheckInDate).NotEqual(DateOnly.MinValue);
        RuleFor(request => request).Must(HasMeaningfulContent)
            .WithMessage("A body check-in must contain at least one measurement, wellbeing score, note, or photo.");
        RuleFor(request => request.PhotoMediaId).NotEqual(Guid.Empty).When(request => request.PhotoMediaId.HasValue);
    }

    private static bool HasMeaningfulContent(CreateBodyCheckInRequest request) =>
        HasAnyInput(request) || request.PhotoMediaId.HasValue;
}

public class UpdateBodyCheckInRequestValidator : BodyCheckInInputValidator<UpdateBodyCheckInRequest>
{
    public UpdateBodyCheckInRequestValidator()
    {
        RuleFor(request => request.CheckInDate).NotEqual(DateOnly.MinValue);
        RuleFor(request => request.PhotoOperation).IsInEnum();
        RuleFor(request => request.PhotoMediaId).NotNull()
            .When(request => request.PhotoOperation == BodyCheckInPhotoOperation.Replace);
        RuleFor(request => request.PhotoMediaId).Null()
            .When(request => request.PhotoOperation != BodyCheckInPhotoOperation.Replace);
        RuleFor(request => request.PhotoMediaId).NotEqual(Guid.Empty).When(request => request.PhotoMediaId.HasValue);
    }
}

public class BodyCheckInHistoryFilterValidator : AbstractValidator<BodyCheckInHistoryFilter>
{
    public BodyCheckInHistoryFilterValidator()
    {
        RuleFor(filter => filter.Page).GreaterThan(0);
        RuleFor(filter => filter.PageSize).InclusiveBetween(1, 100);
        RuleFor(filter => filter.To).GreaterThanOrEqualTo(filter => filter.From!.Value)
            .When(filter => filter.From.HasValue && filter.To.HasValue);
    }
}

public class BodyCheckInSummaryFilterValidator : AbstractValidator<BodyCheckInSummaryFilter>
{
    public BodyCheckInSummaryFilterValidator()
    {
        RuleFor(filter => filter.To).GreaterThanOrEqualTo(filter => filter.From!.Value)
            .When(filter => filter.From.HasValue && filter.To.HasValue);
        RuleFor(filter => filter).Must(HasSupportedRange)
            .When(filter => filter.From.HasValue && filter.To.HasValue)
            .WithMessage("Summary date range must not exceed 366 days.");
    }

    private static bool HasSupportedRange(BodyCheckInSummaryFilter filter) =>
        filter.To!.Value.DayNumber - filter.From!.Value.DayNumber <= 366;
}

public abstract class BodyCheckInInputValidator<T> : AbstractValidator<T> where T : IBodyCheckInInput
{
    protected BodyCheckInInputValidator()
    {
        RuleFor(request => request.WeightKg).InclusiveBetween(0.01, BodyCheckInLimits.MaximumWeightKg).When(request => request.WeightKg.HasValue);
        RuleFor(request => request.BodyFatPercent).InclusiveBetween(0, BodyCheckInLimits.MaximumBodyFatPercent).When(request => request.BodyFatPercent.HasValue);
        RuleFor(request => request.WaistCm).InclusiveBetween(0.01, BodyCheckInLimits.MaximumCircumferenceCm).When(request => request.WaistCm.HasValue);
        RuleFor(request => request.ChestCm).InclusiveBetween(0.01, BodyCheckInLimits.MaximumCircumferenceCm).When(request => request.ChestCm.HasValue);
        RuleFor(request => request.HipsCm).InclusiveBetween(0.01, BodyCheckInLimits.MaximumCircumferenceCm).When(request => request.HipsCm.HasValue);
        RuleFor(request => request.ArmCm).InclusiveBetween(0.01, BodyCheckInLimits.MaximumCircumferenceCm).When(request => request.ArmCm.HasValue);
        RuleFor(request => request.ThighCm).InclusiveBetween(0.01, BodyCheckInLimits.MaximumCircumferenceCm).When(request => request.ThighCm.HasValue);
        RuleFor(request => request.WellbeingScore).InclusiveBetween(BodyCheckInLimits.MinimumWellbeingScore, BodyCheckInLimits.MaximumWellbeingScore).When(request => request.WellbeingScore.HasValue);
        RuleFor(request => request.Note).MaximumLength(BodyCheckInLimits.MaximumNoteLength).When(request => request.Note is not null);
    }

    protected static bool HasAnyInput(IBodyCheckInInput request) =>
        request.WeightKg.HasValue || request.BodyFatPercent.HasValue || request.WaistCm.HasValue || request.ChestCm.HasValue ||
        request.HipsCm.HasValue || request.ArmCm.HasValue || request.ThighCm.HasValue || request.WellbeingScore.HasValue ||
        !string.IsNullOrWhiteSpace(request.Note);
}
