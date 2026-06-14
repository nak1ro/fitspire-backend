using FluentValidation;
using backend.Modules.Social.Contracts.Comments;
using backend.Modules.Social.Contracts.Posts;

namespace backend.Modules.Social.Validators;

public class CreatePostRequestValidator : AbstractValidator<CreatePostRequest>
{
    public CreatePostRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty()
            .MaximumLength(2000);

        RuleFor(x => x.ImageUrl)
            .MaximumLength(2048)
            .When(x => x.ImageUrl is not null);
    }
}

public class UpdatePostRequestValidator : AbstractValidator<UpdatePostRequest>
{
    public UpdatePostRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty()
            .MaximumLength(2000);

        RuleFor(x => x.ImageUrl)
            .MaximumLength(2048)
            .When(x => x.ImageUrl is not null);
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
