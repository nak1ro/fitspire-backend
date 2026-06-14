using backend.Modules.Notification.Domain.Constants;
using backend.Modules.Notification.Domain.Enums;
using backend.Modules.Notification.Services;
using backend.Data;
using backend.Modules.Social.Contracts.Posts;
using backend.Modules.Social.Domain;
using backend.Modules.Social.Infrastructure;
using backend.Modules.Shared;
using backend.Modules.Shared.Domain;
using MediatR;

namespace backend.Modules.Social.Features.Follow;

public record FollowUserCommand(Guid FollowerId, Guid FollowedId) : IRequest<FollowResponse>;

public class FollowUserHandler : IRequestHandler<FollowUserCommand, FollowResponse>
{
    private readonly ISocialRepository _socialRepository;
    private readonly INotificationService _notificationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly FitspireDbContext _context;

    public FollowUserHandler(
        ISocialRepository socialRepository,
        INotificationService notificationService,
        IUnitOfWork unitOfWork,
        FitspireDbContext context)
    {
        _socialRepository = socialRepository;
        _notificationService = notificationService;
        _unitOfWork = unitOfWork;
        _context = context;
    }

    public async Task<FollowResponse> Handle(FollowUserCommand request, CancellationToken cancellationToken)
    {
        if (request.FollowerId == request.FollowedId)
            throw new DomainException("Users cannot follow themselves.");

        if (!await _socialRepository.UserExistsAsync(request.FollowedId, cancellationToken))
            throw new NotFoundException($"User {request.FollowedId} not found.");

        var existingFollow = await _socialRepository.GetFollowAsync(request.FollowerId, request.FollowedId, cancellationToken);
        if (existingFollow is not null)
            return new FollowResponse(true);

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            FollowResponse response;
            if (await _socialRepository.IsUserPrivateAsync(request.FollowedId, cancellationToken))
            {
                response = await RequestPrivateFollowAsync(request, cancellationToken);
            }
            else
            {
                var pendingRequest = await _socialRepository.GetPendingFollowRequestAsync(
                    request.FollowerId,
                    request.FollowedId,
                    cancellationToken);
                pendingRequest?.Cancel();

                await _socialRepository.AddFollowerAsync(
                    new Follower(request.FollowerId, request.FollowedId),
                    cancellationToken);
                await CreateFollowNotificationAsync(request, cancellationToken);
                response = new FollowResponse(true);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return response;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private async Task<FollowResponse> RequestPrivateFollowAsync(FollowUserCommand request, CancellationToken cancellationToken)
    {
        var existingRequest = await _socialRepository.GetPendingFollowRequestAsync(
            request.FollowerId, request.FollowedId, cancellationToken);
        if (existingRequest is not null)
            return new FollowResponse(false, true);

        await _socialRepository.AddFollowRequestAsync(new FollowRequest(request.FollowerId, request.FollowedId), cancellationToken);
        var followerName = await _socialRepository.GetUserDisplayNameAsync(request.FollowerId, cancellationToken);
        await _notificationService.CreateAsync(
            request.FollowedId,
            NotificationType.FollowRequest,
            $"{followerName} requested to follow you.",
            request.FollowerId,
            request.FollowerId,
            NotificationReferenceTypes.User,
            cancellationToken);
        return new FollowResponse(false, true);
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
