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

        var jsonData = JsonSerializer.Serialize(WorkoutRoutineSnapshot.Create(workout));

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

internal static class WorkoutRoutineSnapshot
{
    public static object Create(UserWorkout workout) => workout switch
    {
        GymUserWorkoutDetails gym => new
        {
            SchemaVersion = 1, gym.WorkoutType, gym.SplitType, gym.IntensityLevel, gym.DurationMinutes, gym.Notes, gym.IsPrivate, gym.CaloriesBurned,
            Exercises = gym.Exercises.OrderBy(exercise => exercise.OrderIndex).Select(exercise => new
            {
                exercise.ExerciseId,
                exercise.Notes,
                Sets = exercise.WorkoutSets.OrderBy(set => set.OrderIndex).Select(set => new
                {
                    set.Reps,
                    set.WeightKg,
                    set.DurationSeconds,
                    set.DistanceMeters,
                    set.IsWarmup,
                    set.Rpe,
                    set.Notes
                })
            })
        },
        RunningUserWorkoutDetails running => new { SchemaVersion = 1, running.WorkoutType, running.DistanceKm, running.ElevationGainMeters, running.StepCount, running.MapData, running.DurationMinutes, running.Notes, running.IsPrivate, running.CaloriesBurned },
        CyclingUserWorkoutDetails cycling => new { SchemaVersion = 1, cycling.WorkoutType, cycling.DistanceKm, cycling.ElevationGainMeters, cycling.MapData, cycling.IsIndoor, cycling.DurationMinutes, cycling.Notes, cycling.IsPrivate, cycling.CaloriesBurned },
        SwimmingUserWorkoutDetails swimming => new { SchemaVersion = 1, swimming.WorkoutType, swimming.Laps, swimming.PoolLengthMeters, swimming.DistanceMeters, swimming.StrokeType, swimming.DurationMinutes, swimming.Notes, swimming.IsPrivate, swimming.CaloriesBurned },
        YogaUserWorkoutDetails yoga => new { SchemaVersion = 1, yoga.WorkoutType, yoga.Style, yoga.Intensity, yoga.FocusArea, yoga.DurationMinutes, yoga.Notes, yoga.IsPrivate, yoga.CaloriesBurned },
        _ => throw new DomainException($"Unsupported workout type: {workout.WorkoutType}")
    };
}
