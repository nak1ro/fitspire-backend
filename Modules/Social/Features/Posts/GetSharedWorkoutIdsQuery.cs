using backend.Modules.Social.Infrastructure;
using MediatR;

namespace backend.Modules.Social.Features.Posts;

public record GetSharedWorkoutIdsQuery(Guid UserId) : IRequest<List<Guid>>;

public class GetSharedWorkoutIdsHandler : IRequestHandler<GetSharedWorkoutIdsQuery, List<Guid>>
{
    private readonly ISocialRepository _socialRepository;

    public GetSharedWorkoutIdsHandler(ISocialRepository socialRepository)
    {
        _socialRepository = socialRepository;
    }

    public Task<List<Guid>> Handle(GetSharedWorkoutIdsQuery request, CancellationToken cancellationToken) =>
        _socialRepository.GetSharedWorkoutIdsAsync(request.UserId, cancellationToken);
}
