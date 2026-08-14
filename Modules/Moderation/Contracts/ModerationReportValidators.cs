using backend.Modules.Moderation.Domain;
using FluentValidation;

namespace backend.Modules.Moderation.Contracts;

public sealed class CreateModerationReportRequestValidator : AbstractValidator<CreateModerationReportRequest>
{
    public CreateModerationReportRequestValidator()
    {
        RuleFor(request => request.TargetType).IsInEnum();
        RuleFor(request => request.TargetId).NotEmpty();
        RuleFor(request => request.Reason).IsInEnum();
        RuleFor(request => request.Details)
            .MaximumLength(ModerationLimits.MaximumReportDetailsLength)
            .When(request => request.Details is not null);
    }
}

public sealed class AdminModerationReportFilterValidator : AbstractValidator<AdminModerationReportFilter>
{
    public AdminModerationReportFilterValidator()
    {
        RuleFor(filter => filter.Status).IsInEnum().When(filter => filter.Status is not null);
        RuleFor(filter => filter.TargetType).IsInEnum().When(filter => filter.TargetType is not null);
        RuleFor(filter => filter.Reason).IsInEnum().When(filter => filter.Reason is not null);
        RuleFor(filter => filter.Page).GreaterThan(0);
        RuleFor(filter => filter.PageSize).InclusiveBetween(1, 100);
    }
}

public sealed class ResolveModerationReportRequestValidator : AbstractValidator<ResolveModerationReportRequest>
{
    public ResolveModerationReportRequestValidator()
    {
        RuleFor(request => request.Action).IsInEnum();
        RuleFor(request => request.ModeratorNote)
            .MaximumLength(ModerationLimits.MaximumResolutionNoteLength)
            .When(request => request.ModeratorNote is not null);
        RuleFor(request => request.SuspensionDurationDays)
            .InclusiveBetween(1, ModerationLimits.MaximumSuspensionDurationDays)
            .When(RequiresSuspension);
        RuleFor(request => request.SuspensionDurationDays)
            .Null()
            .When(request => !RequiresSuspension(request));
    }

    private static bool RequiresSuspension(ResolveModerationReportRequest request) =>
        request.Action is AdminModerationResolutionAction.SuspendUser or AdminModerationResolutionAction.RemoveTargetAndSuspendUser;
}
