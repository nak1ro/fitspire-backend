using backend.Modules.Social.Domain;
using backend.Modules.Social.Infrastructure;
using backend.Modules.Shared;
using MediatR;

namespace backend.Modules.Social.Features.Follow;

public record FollowUserCommand(Guid FollowerId, Guid FollowedId) : IRequest<bool>;

public class FollowUserHandler : IRequestHandler<FollowUserCommand, bool>
{
    private readonly ISocialRepository _socialRepository;
    private readonly IUnitOfWork _unitOfWork;

    public FollowUserHandler(ISocialRepository socialRepository, IUnitOfWork unitOfWork)
    {
        _socialRepository = socialRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(FollowUserCommand request, CancellationToken cancellationToken)
    {
        // Check if already following
        var existingFollow = await _socialRepository.GetFollowAsync(request.FollowerId, request.FollowedId, cancellationToken);
        
        if (existingFollow != null)
        {
            // Unfollow
            await _socialRepository.RemoveFollowerAsync(existingFollow, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return false; // Unfollowed
        }

        // Follow
        var follower = new Follower(request.FollowerId, request.FollowedId);
        await _socialRepository.AddFollowerAsync(follower, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return true; // Now following
    }
}
