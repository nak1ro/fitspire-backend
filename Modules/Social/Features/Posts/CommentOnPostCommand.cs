using backend.Modules.Notification.Domain.Constants;
using backend.Modules.Notification.Domain.Enums;
using backend.Modules.Notification.Services;
using backend.Modules.Social.Domain;
using backend.Modules.Social.Infrastructure;
using backend.Modules.Shared;
using backend.Modules.Shared.Domain;
using backend.Modules.Social.Services;
using MediatR;

namespace backend.Modules.Social.Features.Posts;

public record CommentOnPostCommand(Guid UserId, Guid PostId, string Content, Guid? ReplyToCommentId = null) : IRequest<Guid>;

public class CommentOnPostHandler : IRequestHandler<CommentOnPostCommand, Guid>
{
    private readonly ISocialRepository _socialRepository;
    private readonly INotificationService _notificationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISocialAccessService _socialAccessService;

    public CommentOnPostHandler(
        ISocialRepository socialRepository,
        INotificationService notificationService,
        IUnitOfWork unitOfWork,
        ISocialAccessService socialAccessService)
    {
        _socialRepository = socialRepository;
        _notificationService = notificationService;
        _unitOfWork = unitOfWork;
        _socialAccessService = socialAccessService;
    }

    public async Task<Guid> Handle(CommentOnPostCommand request, CancellationToken cancellationToken)
    {
        var post = await _socialRepository.GetPostByIdAsync(request.PostId, cancellationToken);
        if (post is null)
            throw new NotFoundException($"Post {request.PostId} not found.");

        if (!await _socialAccessService.CanViewProtectedContentAsync(
                request.UserId,
                post.UserId,
                cancellationToken))
            throw new NotFoundException($"Post {request.PostId} not found.");

        var replyTarget = request.ReplyToCommentId.HasValue
            ? await GetReplyTargetAsync(request, cancellationToken)
            : null;
        var comment = replyTarget is null
            ? new Comment(request.PostId, request.UserId, request.Content)
            : Comment.CreateReply(replyTarget, request.UserId, request.Content);

        await _socialRepository.AddCommentAsync(comment, cancellationToken);
        await CreateCommentNotificationsAsync(post.UserId, replyTarget?.UserId, comment.Id, request, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return comment.Id;
    }

    private async Task<Comment> GetReplyTargetAsync(CommentOnPostCommand request, CancellationToken cancellationToken)
    {
        var targetComment = await _socialRepository.GetCommentByIdAsync(
            request.PostId, request.ReplyToCommentId!.Value, cancellationToken)
            ?? throw new NotFoundException($"Comment {request.ReplyToCommentId} not found.");

        return targetComment;
    }

    private async Task CreateCommentNotificationsAsync(
        Guid postOwnerId,
        Guid? replyTargetUserId,
        Guid commentId,
        CommentOnPostCommand request,
        CancellationToken cancellationToken)
    {
        var actorName = await _socialRepository.GetUserDisplayNameAsync(request.UserId, cancellationToken);
        if (postOwnerId != request.UserId && postOwnerId != replyTargetUserId)
        {
            await _notificationService.CreateAsync(
                postOwnerId,
                NotificationType.PostComment,
                $"{actorName} commented on your post.",
                request.UserId,
                request.PostId,
                NotificationReferenceTypes.Post,
                cancellationToken);
        }

        if (replyTargetUserId is not null && replyTargetUserId != request.UserId)
        {
            await _notificationService.CreateAsync(
                replyTargetUserId.Value,
                NotificationType.CommentReply,
                $"{actorName} replied to your comment.",
                request.UserId,
                commentId,
                NotificationReferenceTypes.Comment,
                cancellationToken);
        }
    }
}
