using backend.Modules.Notification.Domain.Constants;
using backend.Modules.Notification.Domain.Enums;
using backend.Modules.Notification.Services;
using backend.Modules.Social.Domain;
using backend.Modules.Social.Infrastructure;
using backend.Modules.Shared;
using backend.Modules.Shared.Domain;
using MediatR;

namespace backend.Modules.Social.Features.Posts;

public record CommentOnPostCommand(Guid UserId, Guid PostId, string Content) : IRequest<Guid>;

public class CommentOnPostHandler : IRequestHandler<CommentOnPostCommand, Guid>
{
    private readonly ISocialRepository _socialRepository;
    private readonly INotificationService _notificationService;
    private readonly IUnitOfWork _unitOfWork;

    public CommentOnPostHandler(
        ISocialRepository socialRepository,
        INotificationService notificationService,
        IUnitOfWork unitOfWork)
    {
        _socialRepository = socialRepository;
        _notificationService = notificationService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CommentOnPostCommand request, CancellationToken cancellationToken)
    {
        var post = await _socialRepository.GetPostByIdAsync(request.PostId, cancellationToken);
        if (post is null)
            throw new NotFoundException($"Post {request.PostId} not found.");

        var comment = new Comment(request.PostId, request.UserId, request.Content);

        await _socialRepository.AddCommentAsync(comment, cancellationToken);
        await CreatePostCommentNotificationAsync(post.UserId, request, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return comment.Id;
    }

    private async Task CreatePostCommentNotificationAsync(
        Guid postOwnerId,
        CommentOnPostCommand request,
        CancellationToken cancellationToken)
    {
        if (postOwnerId == request.UserId)
            return;

        var actorName = await _socialRepository.GetUserDisplayNameAsync(request.UserId, cancellationToken);
        await _notificationService.CreateAsync(
            postOwnerId,
            NotificationType.PostComment,
            $"{actorName} commented on your post.",
            request.UserId,
            request.PostId,
            NotificationReferenceTypes.Post,
            cancellationToken);
    }
}
