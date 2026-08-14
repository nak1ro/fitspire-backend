using backend.Modules.AiCoaching.Domain;
using FluentValidation;

namespace backend.Modules.AiCoaching.Contracts;

public sealed class CreateCoachThreadRequestValidator : AbstractValidator<CreateCoachThreadRequest>
{
    public CreateCoachThreadRequestValidator() => RuleFor(request => request.Title)
        .MaximumLength(AiCoachInteractionLimits.MaximumThreadTitleLength);
}

public sealed class UpdateCoachThreadRequestValidator : AbstractValidator<UpdateCoachThreadRequest>
{
    public UpdateCoachThreadRequestValidator() => RuleFor(request => request.Title).NotEmpty()
        .MaximumLength(AiCoachInteractionLimits.MaximumThreadTitleLength);
}

public sealed class CoachThreadHistoryFilterValidator : AbstractValidator<CoachThreadHistoryFilter>
{
    public CoachThreadHistoryFilterValidator()
    {
        RuleFor(filter => filter.Page).InclusiveBetween(1, 10_000);
        RuleFor(filter => filter.PageSize).InclusiveBetween(1, 50);
    }
}

public sealed class SendCoachMessageRequestValidator : AbstractValidator<SendCoachMessageRequest>
{
    public SendCoachMessageRequestValidator()
    {
        RuleFor(request => request.ClientRequestId).NotEmpty();
        RuleFor(request => request.Content).NotEmpty().MaximumLength(AiCoachInteractionLimits.MaximumQuestionLength);
    }
}

public sealed class CoachMessageHistoryFilterValidator : AbstractValidator<CoachMessageHistoryFilter>
{
    public CoachMessageHistoryFilterValidator()
    {
        RuleFor(filter => filter.BeforeSequence).GreaterThan(0).When(filter => filter.BeforeSequence.HasValue);
        RuleFor(filter => filter.PageSize).InclusiveBetween(1, 50);
    }
}
