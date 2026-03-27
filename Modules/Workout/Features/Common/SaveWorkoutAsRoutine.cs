using System.Text.Json;
using backend.Modules.Shared;
using backend.Modules.Shared.Domain;
using backend.Modules.Workout.Domain.Entities;
using backend.Modules.Workout.DTOs;
using backend.Modules.Workout.Infrastructure;
using MediatR;

namespace backend.Modules.Workout.Features.Common;

public record SaveWorkoutAsRoutineCommand(
    Guid CurrentUserId, 
    Guid WorkoutId, 
    string RoutineName,
    string? Description
) : IRequest<Guid>;

public class SaveWorkoutAsRoutineHandler : IRequestHandler<SaveWorkoutAsRoutineCommand, Guid>
{
    private readonly IWorkoutRepository _workoutRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SaveWorkoutAsRoutineHandler(IWorkoutRepository workoutRepository, IUnitOfWork unitOfWork)
    {
        _workoutRepository = workoutRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(SaveWorkoutAsRoutineCommand request, CancellationToken cancellationToken)
    {
        // 1. Get existing workout
        var workout = await _workoutRepository.GetGymWorkoutByIdAsync(request.WorkoutId, cancellationToken) 
                      ?? await _workoutRepository.GetByIdAsync(request.WorkoutId, cancellationToken);

        if (workout == null)
            throw new NotFoundException($"Workout {request.WorkoutId} not found.");

        if (workout.UserId != request.CurrentUserId)
            throw new UnauthorizedAccessException("Cannot save another user's workout as routine.");

        // 2. Serialize workout data to JSON (simple snapshot)
        // In a real app we might map to a cleaner DTO first to avoid circular refs or unnecessary fields,
        // but for now verifying concept with direct serialization (ignoring cycles).
        var jsonOptions = new JsonSerializerOptions { ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles };
        var jsonData = JsonSerializer.Serialize(workout, workout.GetType(), jsonOptions);

        // 3. Create Routine
        var routine = new WorkoutRoutine(
            Guid.NewGuid(),
            request.CurrentUserId,
            request.RoutineName,
            workout.WorkoutType,
            jsonData,
            request.Description
        );

        // 4. Save
        await _workoutRepository.AddRoutineAsync(routine, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return routine.Id;
    }
}
