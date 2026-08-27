using FluentValidation;
using backend.Modules.Social.Contracts.Comments;
using backend.Modules.Social.Contracts.Posts;

namespace backend.Modules.Social.Validators;

public class CreatePostRequestValidator : AbstractValidator<CreatePostRequest>
{
    public CreatePostRequestValidator()
    {
        RuleFor(x => x.Content)
            .MaximumLength(2000)
            .When(x => x.Content is not null);

        RuleFor(x => x.MediaAssetIds)
            .Must(ids => ids is null || ids.Count <= 10)
            .WithMessage("A post can contain at most ten images.")
            .Must(ids => ids is null || ids.All(id => id != Guid.Empty) && ids.Distinct().Count() == ids.Count)
            .WithMessage("Post media IDs must be unique and non-empty.");

        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.Content) || x.MediaAssetIds is { Count: > 0 })
            .WithMessage("A post needs text or at least one image.");
    }
}

public class UpdatePostRequestValidator : AbstractValidator<UpdatePostRequest>
{
    public UpdatePostRequestValidator()
    {
        RuleFor(x => x.Content)
            .MaximumLength(2000)
            .When(x => x.Content is not null);

        RuleFor(x => x.MediaAssetIds)
            .Must(ids => ids is null || ids.Count <= 10)
            .WithMessage("A post can contain at most ten images.")
            .Must(ids => ids is null || ids.All(id => id != Guid.Empty) && ids.Distinct().Count() == ids.Count)
            .WithMessage("Post media IDs must be unique and non-empty.");

        RuleFor(x => x)
            .Must(x => x.Content is not null || x.MediaAssetIds is not null)
            .WithMessage("At least one post field must be provided.");
    }
}

public class ShareWorkoutRequestValidator : AbstractValidator<ShareWorkoutRequest>
{
    public ShareWorkoutRequestValidator()
    {
        RuleFor(x => x.WorkoutId)
            .NotEmpty();

        RuleFor(x => x.Caption)
            .MaximumLength(2000)
            .When(x => x.Caption is not null);

        RuleFor(x => x.MediaAssetIds)
            .Must(ids => ids is null || ids.Count <= 10)
            .WithMessage("A post can contain at most ten images.")
            .Must(ids => ids is null || ids.All(id => id != Guid.Empty) && ids.Distinct().Count() == ids.Count)
            .WithMessage("Post media IDs must be unique and non-empty.");
    }
}

public class ShareGoalRequestValidator : AbstractValidator<ShareGoalRequest>
{
    public ShareGoalRequestValidator()
    {
        RuleFor(x => x.GoalId)
            .NotEmpty();

        RuleFor(x => x.Caption)
            .MaximumLength(2000)
            .When(x => x.Caption is not null);

        RuleFor(x => x.MediaAssetIds)
            .Must(ids => ids is null || ids.Count <= 10)
            .WithMessage("A post can contain at most ten images.")
            .Must(ids => ids is null || ids.All(id => id != Guid.Empty) && ids.Distinct().Count() == ids.Count)
            .WithMessage("Post media IDs must be unique and non-empty.");
    }
}

public class SharePersonalRecordRequestValidator : AbstractValidator<SharePersonalRecordRequest>
{
    public SharePersonalRecordRequestValidator()
    {
        RuleFor(x => x.PersonalRecordId)
            .NotEmpty();

        RuleFor(x => x.Caption)
            .MaximumLength(2000)
            .When(x => x.Caption is not null);

        RuleFor(x => x.MediaAssetIds)
            .Must(ids => ids is null || ids.Count <= 10)
            .WithMessage("A post can contain at most ten images.")
            .Must(ids => ids is null || ids.All(id => id != Guid.Empty) && ids.Distinct().Count() == ids.Count)
            .WithMessage("Post media IDs must be unique and non-empty.");
    }
}

public class CommentRequestValidator : AbstractValidator<CommentRequest>
{
    public CommentRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty()
            .MaximumLength(1000);
    }
}

public class UpdateCommentRequestValidator : AbstractValidator<UpdateCommentRequest>
{
    public UpdateCommentRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty()
            .MaximumLength(1000);
    }
}
