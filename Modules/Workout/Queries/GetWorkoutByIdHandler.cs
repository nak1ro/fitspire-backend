using backend.Modules.Workout.Domain;
using MediatR;

namespace backend.Modules.Workout.Queries;

public class GetWorkoutByIdHandler : IRequestHandler<GetWorkoutByIdQuery, UserWorkout?>
{
    private readonly IWorkoutRepository _workoutRepository;

    public GetWorkoutByIdHandler(IWorkoutRepository workoutRepository)
    {
        _workoutRepository = workoutRepository;
    }

    public async Task<UserWorkout?> Handle(GetWorkoutByIdQuery request, CancellationToken cancellationToken)
    {
        return await _workoutRepository.GetByIdAsync(request.WorkoutId, cancellationToken);
    }
}
