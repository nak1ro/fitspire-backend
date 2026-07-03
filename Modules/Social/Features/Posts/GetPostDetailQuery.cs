using backend.Modules.Shared.Domain;
using backend.Modules.Social.Features.Common;
using backend.Modules.Social.Infrastructure;
using backend.Modules.Social.Services;
using backend.Modules.Workout.Infrastructure;
using backend.Modules.Media.Contracts;
using MediatR;

namespace backend.Modules.Social.Features.Posts;

public record GetPostDetailQuery(Guid ViewerUserId, Guid PostId) : IRequest<FeedItemResponse>;

public class GetPostDetailHandler : IRequestHandler<GetPostDetailQuery, FeedItemResponse>
{
    private readonly ISocialRepository _repository;
    private readonly ISocialAccessService _accessService;
    private readonly IWorkoutRepository _workoutRepository;
    private readonly IMediaResponseFactory _mediaResponseFactory;

    public GetPostDetailHandler(
        ISocialRepository repository,
        ISocialAccessService accessService,
        IWorkoutRepository workoutRepository,
        IMediaResponseFactory mediaResponseFactory)
    {
        _repository = repository;
        _accessService = accessService;
        _workoutRepository = workoutRepository;
        _mediaResponseFactory = mediaResponseFactory;
    }

    public async Task<FeedItemResponse> Handle(GetPostDetailQuery request, CancellationToken cancellationToken)
    {
        var post = await _repository.GetPostByIdAsync(request.PostId, cancellationToken)
            ?? throw new NotFoundException($"Post {request.PostId} not found.");

        if (!await _accessService.CanViewProtectedContentAsync(request.ViewerUserId, post.UserId, cancellationToken))
            throw new NotFoundException($"Post {request.PostId} not found.");

        return (await PostResponseMapper.MapAsync([post], request.ViewerUserId, _workoutRepository, _mediaResponseFactory, cancellationToken)).Single();
    }
}
