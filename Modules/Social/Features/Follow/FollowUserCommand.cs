using backend.Modules.Notification.Domain.Constants;
using backend.Modules.Notification.Domain.Enums;
using backend.Modules.Notification.Services;
using backend.Modules.Social.Domain;
using backend.Modules.Social.Infrastructure;
using backend.Modules.Shared;
using backend.Modules.Shared.Domain;
using MediatR;

namespace backend.Modules.Social.Features.Follow;

public record FollowUserCommand(Guid FollowerId, Guid FollowedId) : IRequest<bool>;

public class FollowUserHandler : IRequestHandler<FollowUserCommand, bool>
{
    private readonly ISocialRepository _socialRepository;
    private readonly INotificationService _notificationService;
    private readonly IUnitOfWork _unitOfWork;

    public FollowUserHandler(
        ISocialRepository socialRepository,
        INotificationService notificationService,
        IUnitOfWork unitOfWork)
    {
        _socialRepository = socialRepository;
        _notificationService = notificationService;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(FollowUserCommand request, CancellationToken cancellationToken)
    {
        if (request.FollowerId == request.FollowedId)
            throw new DomainException("Users cannot follow themselves.");

        if (!await _socialRepository.UserExistsAsync(request.FollowedId, cancellationToken))
            throw new NotFoundException($"User {request.FollowedId} not found.");

        var existingFollow = await _socialRepository.GetFollowAsync(request.FollowerId, request.FollowedId, cancellationToken);
        
        if (existingFollow != null)
        {
            await _socialRepository.RemoveFollowerAsync(existingFollow, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return false;
        }

        var follower = new Follower(request.FollowerId, request.FollowedId);
        await _socialRepository.AddFollowerAsync(follower, cancellationToken);
        await CreateFollowNotificationAsync(request, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return true;
    }

    private async Task CreateFollowNotificationAsync(
        FollowUserCommand request,
        CancellationToken cancellationToken)
    {
        var followerName = await _socialRepository.GetUserDisplayNameAsync(request.FollowerId, cancellationToken);
        await _notificationService.CreateAsync(
            request.FollowedId,
            NotificationType.Follow,
            $"{followerName} started following you.",
            request.FollowerId,
            request.FollowerId,
            NotificationReferenceTypes.User,
            cancellationToken);
    }
}
