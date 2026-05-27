using backend.Modules.Notification.Domain.Constants;
using backend.Modules.Notification.Domain.Enums;
using backend.Modules.Notification.Services;
using backend.Modules.Social.Domain;
using backend.Modules.Social.Infrastructure;
using backend.Modules.Shared;
using backend.Modules.Shared.Domain;
using MediatR;

namespace backend.Modules.Social.Features.Posts;

public record LikePostCommand(Guid UserId, Guid PostId) : IRequest<bool>;

public class LikePostHandler : IRequestHandler<LikePostCommand, bool>
{
    private readonly ISocialRepository _socialRepository;
    private readonly INotificationService _notificationService;
    private readonly IUnitOfWork _unitOfWork;

    public LikePostHandler(
        ISocialRepository socialRepository,
        INotificationService notificationService,
        IUnitOfWork unitOfWork)
    {
        _socialRepository = socialRepository;
        _notificationService = notificationService;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(LikePostCommand request, CancellationToken cancellationToken)
    {
        var post = await _socialRepository.GetPostByIdAsync(request.PostId, cancellationToken);
        if (post is null)
            throw new NotFoundException($"Post {request.PostId} not found.");

        var existingLike = await _socialRepository.GetLikeAsync(request.UserId, request.PostId, cancellationToken);
        
        if (existingLike != null)
        {
            await _socialRepository.RemoveLikeAsync(existingLike, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return false;
        }

        var like = Like.CreateForPost(request.UserId, request.PostId);
        await _socialRepository.AddLikeAsync(like, cancellationToken);
        await CreatePostLikeNotificationAsync(post.UserId, request, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return true;
    }

    private async Task CreatePostLikeNotificationAsync(
        Guid postOwnerId,
        LikePostCommand request,
        CancellationToken cancellationToken)
    {
        if (postOwnerId == request.UserId)
            return;

        var actorName = await _socialRepository.GetUserDisplayNameAsync(request.UserId, cancellationToken);
        await _notificationService.CreateAsync(
            postOwnerId,
            NotificationType.PostLike,
            $"{actorName} liked your post.",
            request.UserId,
            request.PostId,
            NotificationReferenceTypes.Post,
            cancellationToken);
    }
}
