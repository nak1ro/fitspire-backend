using backend.Modules.Workout.Domain.Entities;
using backend.Modules.Workout.Infrastructure;
using MediatR;

namespace backend.Modules.Workout.Features.Common;

public record GetWorkoutByIdQuery(Guid WorkoutId, Guid UserId) : IRequest<UserWorkout?>;

public class GetWorkoutByIdHandler : IRequestHandler<GetWorkoutByIdQuery, UserWorkout?>
{
    private readonly IWorkoutRepository _workoutRepository;

    public GetWorkoutByIdHandler(IWorkoutRepository workoutRepository)
    {
        _workoutRepository = workoutRepository;
    }

    public async Task<UserWorkout?> Handle(GetWorkoutByIdQuery request, CancellationToken cancellationToken)
    {
        var workout = await _workoutRepository.GetDetailsByIdAsync(request.WorkoutId, cancellationToken);
        return workout?.UserId == request.UserId ? workout : null;
    }
}
