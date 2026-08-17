using backend.Modules.Social.Infrastructure;
using MediatR;

namespace backend.Modules.Social.Features.Posts;

public record GetSharedGoalIdsQuery(Guid UserId) : IRequest<List<Guid>>;

public class GetSharedGoalIdsHandler : IRequestHandler<GetSharedGoalIdsQuery, List<Guid>>
{
    private readonly ISocialRepository _socialRepository;

    public GetSharedGoalIdsHandler(ISocialRepository socialRepository)
    {
        _socialRepository = socialRepository;
    }

    public Task<List<Guid>> Handle(GetSharedGoalIdsQuery request, CancellationToken cancellationToken) =>
        _socialRepository.GetSharedGoalIdsAsync(request.UserId, cancellationToken);
}
