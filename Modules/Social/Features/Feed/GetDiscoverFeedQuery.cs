using backend.Modules.Social.Features.Common;
using backend.Modules.Social.Infrastructure;
using backend.Modules.Workout.Infrastructure;
using backend.Modules.Media.Contracts;
using MediatR;

namespace backend.Modules.Social.Features.Feed;

public record GetDiscoverFeedQuery(Guid ViewerUserId, int Page, int PageSize) : IRequest<List<FeedItemResponse>>;

public class GetDiscoverFeedHandler : IRequestHandler<GetDiscoverFeedQuery, List<FeedItemResponse>>
{
    private readonly ISocialRepository _repository;
    private readonly IWorkoutRepository _workoutRepository;
    private readonly IMediaResponseFactory _mediaResponseFactory;

    public GetDiscoverFeedHandler(
        ISocialRepository repository,
        IWorkoutRepository workoutRepository,
        IMediaResponseFactory mediaResponseFactory)
    {
        _repository = repository;
        _workoutRepository = workoutRepository;
        _mediaResponseFactory = mediaResponseFactory;
    }

    public async Task<List<FeedItemResponse>> Handle(GetDiscoverFeedQuery request, CancellationToken cancellationToken)
    {
        var posts = await _repository.GetDiscoverFeedAsync(request.Page, request.PageSize, cancellationToken);
        return await PostResponseMapper.MapAsync(posts, request.ViewerUserId, _workoutRepository, _mediaResponseFactory, cancellationToken);
    }
}
