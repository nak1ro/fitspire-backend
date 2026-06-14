using backend.Modules.Notification.Domain.Enums;
using backend.Modules.Notification.Services;
using backend.Modules.Shared;
using backend.Modules.Shared.Domain;
using backend.Modules.Social.Domain;
using backend.Modules.Social.Infrastructure;
using backend.Modules.Social.Services;
using backend.Modules.Social.Contracts.Posts;
using backend.Modules.Notification.Domain.Constants;
using MediatR;

namespace backend.Modules.Social.Features.Posts;

public record LikeCommentCommand(Guid UserId, Guid PostId, Guid CommentId, bool? IsLiked = null) : IRequest<LikeResponse>;

public class LikeCommentHandler : IRequestHandler<LikeCommentCommand, LikeResponse>
{
    private readonly ISocialRepository _repository;
    private readonly ISocialAccessService _accessService;
    private readonly INotificationService _notifications;
    private readonly IUnitOfWork _unitOfWork;

    public LikeCommentHandler(ISocialRepository repository, ISocialAccessService accessService, INotificationService notifications, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _accessService = accessService;
        _notifications = notifications;
        _unitOfWork = unitOfWork;
    }

    public async Task<LikeResponse> Handle(LikeCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = await _repository.GetCommentByIdAsync(request.PostId, request.CommentId, cancellationToken)
            ?? throw new NotFoundException($"Comment {request.CommentId} not found.");
        if (!await _accessService.CanViewProtectedContentAsync(request.UserId, comment.Post.UserId, cancellationToken))
            throw new NotFoundException($"Comment {request.CommentId} not found.");

        var existing = await _repository.GetCommentLikeAsync(request.UserId, request.CommentId, cancellationToken);
        if (existing is not null)
        {
            if (request.IsLiked == true)
                return new LikeResponse(true);

            await _repository.RemoveCommentLikeAsync(existing, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return new LikeResponse(false);
        }

        if (request.IsLiked == false)
            return new LikeResponse(false);

        await _repository.AddCommentLikeAsync(new CommentLike(request.UserId, request.CommentId), cancellationToken);
        if (comment.UserId != request.UserId)
        {
            var actor = await _repository.GetUserDisplayNameAsync(request.UserId, cancellationToken);
            await _notifications.CreateAsync(comment.UserId, NotificationType.CommentLike, $"{actor} liked your comment.", request.UserId, request.CommentId, NotificationReferenceTypes.Comment, cancellationToken);
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new LikeResponse(true);
    }
}
