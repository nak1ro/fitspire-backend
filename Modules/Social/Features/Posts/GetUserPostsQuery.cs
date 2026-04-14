using backend.Modules.Social.Features.Common;
using backend.Modules.Social.Infrastructure;
using backend.Modules.Shared.Domain;
using backend.Modules.Workout.Infrastructure;
using MediatR;

namespace backend.Modules.Social.Features.Posts;

public record GetUserPostsQuery(Guid ViewerUserId, Guid TargetUserId, int Page = 1, int PageSize = 20) : IRequest<List<FeedItemResponse>>;

public class GetUserPostsHandler : IRequestHandler<GetUserPostsQuery, List<FeedItemResponse>>
{
    private readonly ISocialRepository _socialRepository;
    private readonly IWorkoutRepository _workoutRepository;

    public GetUserPostsHandler(ISocialRepository socialRepository, IWorkoutRepository workoutRepository)
    {
        _socialRepository = socialRepository;
        _workoutRepository = workoutRepository;
    }

    public async Task<List<FeedItemResponse>> Handle(GetUserPostsQuery request, CancellationToken cancellationToken)
    {
        if (!await _socialRepository.UserExistsAsync(request.TargetUserId, cancellationToken))
            throw new NotFoundException($"User {request.TargetUserId} not found.");

        var posts = await _socialRepository.GetUserPostsAsync(
            request.TargetUserId,
            request.Page,
            request.PageSize,
            cancellationToken);

        return await PostResponseMapper.MapAsync(posts, request.ViewerUserId, _workoutRepository, cancellationToken);
    }
}
