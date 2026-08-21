using AutoMapper;
using backend.Modules.Shared.Domain;
using backend.Modules.Social.Services;
using backend.Modules.Workout.Features.Common;
using backend.Modules.Workout.Infrastructure;
using MediatR;

namespace backend.Modules.Social.Features.Profiles;

public record GetPublicWorkoutDetailQuery(Guid ViewerId, Guid OwnerId, Guid WorkoutId) : IRequest<object>;

public class GetPublicWorkoutDetailHandler : IRequestHandler<GetPublicWorkoutDetailQuery, object>
{
    private readonly IWorkoutRepository _workoutRepository;
    private readonly ISocialAccessService _access;
    private readonly IMapper _mapper;

    public GetPublicWorkoutDetailHandler(IWorkoutRepository workoutRepository, ISocialAccessService access, IMapper mapper)
    {
        _workoutRepository = workoutRepository;
        _access = access;
        _mapper = mapper;
    }

    public async Task<object> Handle(GetPublicWorkoutDetailQuery request, CancellationToken cancellationToken)
    {
        // Same quiet-degrade rule as goal detail: any reason this workout isn't viewable
        // (not found, not owned by this user, marked private since it was shared) collapses
        // to the same NotFoundException rather than distinguishing why.
        if (!await _access.CanViewProtectedContentAsync(request.ViewerId, request.OwnerId, cancellationToken))
            throw new NotFoundException("Workout not found.");

        var workout = await _workoutRepository.GetDetailsByIdAsync(request.WorkoutId, cancellationToken);
        if (workout is null || workout.UserId != request.OwnerId || workout.IsPrivate)
            throw new NotFoundException("Workout not found.");

        return WorkoutResponseMapper.Map(workout, _mapper);
    }
}
