using backend.Modules.Notification.Domain.Constants;
using backend.Modules.Notification.Domain.Enums;
using backend.Modules.Notification.Services;
using backend.Modules.Social.Contracts.Posts;
using backend.Modules.Social.Domain;
using backend.Modules.Social.Infrastructure;
using backend.Modules.Shared;
using backend.Modules.Shared.Domain;
using backend.Modules.Social.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace backend.Modules.Social.Features.Posts;

public record LikePostCommand(Guid UserId, Guid PostId, bool? IsLiked = null) : IRequest<LikeResponse>;

public class LikePostHandler : IRequestHandler<LikePostCommand, LikeResponse>
{
    private readonly ISocialRepository _socialRepository;
    private readonly INotificationService _notificationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISocialAccessService _socialAccessService;

    public LikePostHandler(
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

    public async Task<LikeResponse> Handle(LikePostCommand request, CancellationToken cancellationToken)
    {
        var post = await _socialRepository.GetPostByIdAsync(request.PostId, cancellationToken);
        if (post is null)
            throw new NotFoundException($"Post {request.PostId} not found.");

        if (!await _socialAccessService.CanViewProtectedContentAsync(
                request.UserId,
                post.UserId,
                cancellationToken))
            throw new NotFoundException($"Post {request.PostId} not found.");

        var existingLike = await _socialRepository.GetPostLikeAsync(request.UserId, request.PostId, cancellationToken);
        
        if (existingLike != null)
        {
            if (request.IsLiked == true)
                return new LikeResponse(true);

            await _socialRepository.RemovePostLikeAsync(existingLike, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return new LikeResponse(false);
        }

        if (request.IsLiked == false)
            return new LikeResponse(false);

        var like = new PostLike(request.UserId, request.PostId);
        await _socialRepository.AddPostLikeAsync(like, cancellationToken);
        await CreatePostLikeNotificationAsync(post.UserId, request, cancellationToken);
        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            return new LikeResponse(true);
        }
        
        return new LikeResponse(true);
    }

    private static bool IsUniqueViolation(DbUpdateException exception)
    {
        return exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };
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
