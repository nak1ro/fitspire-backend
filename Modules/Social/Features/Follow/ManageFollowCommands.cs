using backend.Data;
using backend.Modules.Notification.Domain.Constants;
using backend.Modules.Notification.Domain.Enums;
using backend.Modules.Notification.Services;
using backend.Modules.Shared;
using backend.Modules.Shared.Domain;
using backend.Modules.Social.Infrastructure;
using MediatR;

namespace backend.Modules.Social.Features.Follow;

public record UnfollowUserCommand(Guid UserId, Guid TargetUserId) : IRequest;
public record RemoveFollowerCommand(Guid UserId, Guid FollowerUserId) : IRequest;
public record CancelFollowRequestCommand(Guid UserId, Guid RequestId) : IRequest;
public record DecideFollowRequestCommand(Guid UserId, Guid RequestId, bool Accept) : IRequest;

public class UnfollowUserHandler : IRequestHandler<UnfollowUserCommand>
{
    private readonly ISocialRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UnfollowUserHandler(ISocialRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UnfollowUserCommand request, CancellationToken cancellationToken)
    {
        var follow = await _repository.GetFollowAsync(request.UserId, request.TargetUserId, cancellationToken);
        if (follow is null)
            return;

        await _repository.RemoveFollowerAsync(follow, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

public class RemoveFollowerHandler : IRequestHandler<RemoveFollowerCommand>
{
    private readonly ISocialRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveFollowerHandler(ISocialRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(RemoveFollowerCommand request, CancellationToken cancellationToken)
    {
        var follow = await _repository.GetFollowAsync(request.FollowerUserId, request.UserId, cancellationToken);
        if (follow is null)
            return;

        await _repository.RemoveFollowerAsync(follow, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

public class CancelFollowRequestHandler : IRequestHandler<CancelFollowRequestCommand>
{
    private readonly ISocialRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelFollowRequestHandler(ISocialRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(CancelFollowRequestCommand request, CancellationToken cancellationToken)
    {
        var followRequest = await _repository.GetFollowRequestAsync(request.RequestId, cancellationToken)
            ?? throw new NotFoundException($"Follow request {request.RequestId} not found.");

        if (followRequest.RequesterId != request.UserId)
            throw new UnauthorizedAccessException("Only the requester can cancel this follow request.");

        followRequest.Cancel();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

public class DecideFollowRequestHandler : IRequestHandler<DecideFollowRequestCommand>
{
    private readonly FitspireDbContext _context;
    private readonly ISocialRepository _repository;
    private readonly INotificationService _notificationService;
    private readonly IUnitOfWork _unitOfWork;

    public DecideFollowRequestHandler(
        FitspireDbContext context,
        ISocialRepository repository,
        INotificationService notificationService,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _repository = repository;
        _notificationService = notificationService;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DecideFollowRequestCommand request, CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        var followRequest = await _repository.GetFollowRequestAsync(request.RequestId, cancellationToken)
            ?? throw new NotFoundException($"Follow request {request.RequestId} not found.");

        if (followRequest.AddresseeId != request.UserId)
            throw new UnauthorizedAccessException("Only the profile owner can decide this follow request.");

        if (request.Accept)
            await AcceptAsync(followRequest, cancellationToken);
        else
            followRequest.Reject();

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private async Task AcceptAsync(Domain.FollowRequest followRequest, CancellationToken cancellationToken)
    {
        followRequest.Accept();
        var existingFollow = await _repository.GetFollowAsync(
            followRequest.RequesterId, followRequest.AddresseeId, cancellationToken);
        if (existingFollow is null)
            await _repository.AddFollowerAsync(new Domain.Follower(followRequest.RequesterId, followRequest.AddresseeId), cancellationToken);

        var ownerName = await _repository.GetUserDisplayNameAsync(followRequest.AddresseeId, cancellationToken);
        await _notificationService.CreateAsync(
            followRequest.RequesterId,
            NotificationType.FollowRequestAccepted,
            $"{ownerName} accepted your follow request.",
            followRequest.AddresseeId,
            followRequest.AddresseeId,
            NotificationReferenceTypes.User,
            cancellationToken);
    }
}
