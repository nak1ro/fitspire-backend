using backend.Modules.Social.Features.Common;
using backend.Modules.Social.Infrastructure;
using backend.Modules.Workout.Infrastructure;
using backend.Modules.Media.Contracts;
using MediatR;

namespace backend.Modules.Social.Features.Feed;

public record GetUserFeedQuery(Guid UserId, int Page = 1, int PageSize = 20) : IRequest<List<FeedItemResponse>>;

public class GetUserFeedHandler : IRequestHandler<GetUserFeedQuery, List<FeedItemResponse>>
{
    private readonly ISocialRepository _socialRepository;
    private readonly IWorkoutRepository _workoutRepository;
    private readonly IMediaResponseFactory _mediaResponseFactory;

    public GetUserFeedHandler(
        ISocialRepository socialRepository,
        IWorkoutRepository workoutRepository,
        IMediaResponseFactory mediaResponseFactory)
    {
        _socialRepository = socialRepository;
        _workoutRepository = workoutRepository;
        _mediaResponseFactory = mediaResponseFactory;
    }

    public async Task<List<FeedItemResponse>> Handle(GetUserFeedQuery request, CancellationToken cancellationToken)
    {
        var posts = await _socialRepository.GetUserFeedAsync(request.UserId, request.Page, request.PageSize, cancellationToken);
        return await PostResponseMapper.MapAsync(posts, request.UserId, _workoutRepository, _mediaResponseFactory, cancellationToken);
    }
}
