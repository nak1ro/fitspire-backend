using backend.Modules.Workout.DTOs;
using backend.Modules.Workout.Infrastructure;
using MediatR;

namespace backend.Modules.Workout.Features.Common;

public record GetWorkoutRoutinesQuery(Guid UserId) : IRequest<List<WorkoutRoutineResponse>>;

public class GetWorkoutRoutinesHandler : IRequestHandler<GetWorkoutRoutinesQuery, List<WorkoutRoutineResponse>>
{
    private readonly IWorkoutRepository _workoutRepository;

    public GetWorkoutRoutinesHandler(IWorkoutRepository workoutRepository)
    {
        _workoutRepository = workoutRepository;
    }

    public async Task<List<WorkoutRoutineResponse>> Handle(GetWorkoutRoutinesQuery request, CancellationToken cancellationToken)
    {
        var routines = await _workoutRepository.GetRoutinesByUserIdAsync(request.UserId, cancellationToken);
        return routines.Select(WorkoutRoutineResponseFactory.Create).ToList();
    }
}
