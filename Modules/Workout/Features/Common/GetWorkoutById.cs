using backend.Modules.Workout.Domain.Entities;
using backend.Modules.Workout.Infrastructure;
using MediatR;

namespace backend.Modules.Workout.Features.Common;

public record GetWorkoutByIdQuery(Guid WorkoutId) : IRequest<UserWorkout?>;

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
