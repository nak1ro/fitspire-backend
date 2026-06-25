using backend.Modules.Shared.Domain;
using backend.Modules.Workout.Infrastructure;
using backend.Modules.Workout.Services;
using MediatR;

namespace backend.Modules.Workout.Features.Common;

public record CompleteWorkoutCommand(
    Guid WorkoutId,
    Guid UserId,
    double? DurationMinutes,
    string? Notes,
    bool? IsPrivate
) : IRequest<bool>;

public class CompleteWorkoutHandler : IRequestHandler<CompleteWorkoutCommand, bool>
{
    private readonly IWorkoutRepository _workoutRepository;
    private readonly IWorkoutDerivedDataService _derivedData;

    public CompleteWorkoutHandler(IWorkoutRepository workoutRepository, IWorkoutDerivedDataService derivedData)
    {
        _workoutRepository = workoutRepository;
        _derivedData = derivedData;
    }

    public async Task<bool> Handle(CompleteWorkoutCommand request, CancellationToken cancellationToken)
    {
        var workout = await _workoutRepository.GetDetailsByIdAsync(request.WorkoutId, cancellationToken);
        
        if (workout is null)
            throw new DomainException($"Workout {request.WorkoutId} not found.");

        if (workout.UserId != request.UserId)
            throw new UnauthorizedAccessException("Workout does not belong to the current user.");

        workout.Complete(request.DurationMinutes, request.Notes, request.IsPrivate);

        await _derivedData.ReconcileCompletedWorkoutAsync(workout, cancellationToken);

        return true;
    }
}
