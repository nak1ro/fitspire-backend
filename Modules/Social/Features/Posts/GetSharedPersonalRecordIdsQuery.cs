using backend.Modules.Social.Infrastructure;
using backend.Modules.Workout.Infrastructure;
using MediatR;

namespace backend.Modules.Social.Features.Posts;

public record GetSharedPersonalRecordIdsQuery(Guid UserId) : IRequest<List<Guid>>;

public class GetSharedPersonalRecordIdsHandler : IRequestHandler<GetSharedPersonalRecordIdsQuery, List<Guid>>
{
    private readonly ISocialRepository _socialRepository;
    private readonly IWorkoutRepository _workoutRepository;

    public GetSharedPersonalRecordIdsHandler(ISocialRepository socialRepository, IWorkoutRepository workoutRepository)
    {
        _socialRepository = socialRepository;
        _workoutRepository = workoutRepository;
    }

    public async Task<List<Guid>> Handle(GetSharedPersonalRecordIdsQuery request, CancellationToken cancellationToken)
    {
        var sharedPairs = await _socialRepository.GetSharedPersonalRecordPairsAsync(request.UserId, cancellationToken);
        var sharedSet = sharedPairs.ToHashSet();

        var currentRecords = await _workoutRepository.GetPersonalRecordsByUserIdAsync(request.UserId, cancellationToken);

        // Only records whose CURRENT (id, achievedAt) has been shared — if a record has since
        // been broken again, its new AchievedAt won't match, and it naturally becomes
        // shareable again without needing to prune old share history.
        return currentRecords
            .Where(record => sharedSet.Contains((record.Id, record.AchievedAt)))
            .Select(record => record.Id)
            .ToList();
    }
}
