using backend.Modules.Shared;
using backend.Modules.Shared.Domain;
using backend.Modules.Workout.DTOs;
using backend.Modules.Workout.Infrastructure;
using backend.Modules.Workout.Services;
using backend.Data;
using backend.Modules.Workout.Domain.Entities;
using backend.Modules.Workout.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace backend.Modules.Workout.Features.Common;

public record UpdateWorkoutCommand(Guid WorkoutId, Guid UserId, UpdateWorkoutRequest Request) : IRequest;

public class UpdateWorkoutHandler : IRequestHandler<UpdateWorkoutCommand>
{
    private readonly IWorkoutRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWorkoutDerivedDataService _derivedData;
    private readonly FitspireDbContext _context;
    private readonly IWorkoutOccurrenceTimeService _occurrenceTimeService;

    public UpdateWorkoutHandler(IWorkoutRepository repository, IUnitOfWork unitOfWork,
        IWorkoutDerivedDataService derivedData, FitspireDbContext context,
        IWorkoutOccurrenceTimeService occurrenceTimeService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _derivedData = derivedData;
        _context = context;
        _occurrenceTimeService = occurrenceTimeService;
    }

    public async Task Handle(UpdateWorkoutCommand request, CancellationToken cancellationToken)
    {
        var workout = await _repository.GetDetailsByIdAsync(request.WorkoutId, cancellationToken);
        
        if (workout == null)
            throw new NotFoundException($"Workout {request.WorkoutId} not found.");

        if (workout.UserId != request.UserId)
            throw new UnauthorizedAccessException("Cannot update another user's workout.");

        var occurredAtUtc = request.Request.Date.HasValue
            ? await _occurrenceTimeService.ResolveUtcAsync(request.UserId, request.Request.Date.Value, cancellationToken)
            : (DateTime?)null;
        workout.UpdateDetails(
            occurredAtUtc,
            request.Request.DurationMinutes,
            request.Request.Notes,
            request.Request.IsPrivate
        );
        if (request.Request.CaloriesBurned.HasValue)
            workout.SetCalories(request.Request.CaloriesBurned.Value);
        await UpdateTypeSpecificFieldsAsync(workout, request.Request, cancellationToken);

        if (workout.Status == Domain.Enums.WorkoutStatus.Completed)
            await _derivedData.ReconcileCompletedWorkoutAsync(workout, cancellationToken);
        else
            await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task UpdateTypeSpecificFieldsAsync(UserWorkout workout, UpdateWorkoutRequest request, CancellationToken cancellationToken)
    {
        switch (workout)
        {
            case GymUserWorkoutDetails gym:
                if (request.SplitType is not null) gym.SetSplitType(Parse<WorkoutSplit>(request.SplitType));
                if (request.IntensityLevel is not null) gym.SetIntensity(Parse<WorkoutIntensity>(request.IntensityLevel));
                if (request.Exercises is not null)
                {
                    var ids = request.Exercises.Select(exercise => exercise.ExerciseId).Distinct().ToList();
                    if (await _context.Exercises.CountAsync(exercise => ids.Contains(exercise.Id), cancellationToken) != ids.Count)
                        throw new DomainException("One or more exercises do not exist.");
                    var existingExercises = gym.Exercises.ToList();
                    _context.GymWorkoutExercises.RemoveRange(existingExercises);
                    gym.ClearExercises();
                    foreach (var exercise in request.Exercises)
                    {
                        var entry = gym.AddExercise(exercise.ExerciseId, exercise.Notes);
                        foreach (var set in exercise.Sets)
                            entry.AddSet(set.Reps, set.WeightKg, set.DurationSeconds, set.DistanceMeters,
                                set.IsWarmup, set.Rpe, set.Notes, set.IsCompleted);
                    }
                }
                break;
            case RunningUserWorkoutDetails running:
                if (request.DistanceKm.HasValue) running.SetDistance(request.DistanceKm.Value);
                if (request.ElevationGainMeters.HasValue || request.StepCount.HasValue || request.MapData is not null)
                    running.SetStats(request.ElevationGainMeters ?? running.ElevationGainMeters, request.StepCount ?? running.StepCount, request.MapData ?? running.MapData);
                break;
            case CyclingUserWorkoutDetails cycling:
                if (request.DistanceKm.HasValue) cycling.SetDistance(request.DistanceKm.Value);
                if (request.ElevationGainMeters.HasValue || request.MapData is not null) cycling.UpdateStats(request.ElevationGainMeters ?? cycling.ElevationGainMeters, request.MapData ?? cycling.MapData);
                if (request.IsIndoor.HasValue) cycling.SetIndoor(request.IsIndoor.Value);
                break;
            case SwimmingUserWorkoutDetails swimming:
                if (request.Laps.HasValue || request.PoolLengthMeters.HasValue) swimming.SetPoolDetails(request.Laps ?? swimming.Laps, request.PoolLengthMeters ?? swimming.PoolLengthMeters);
                if (request.DistanceMeters.HasValue) swimming.SetDistance(request.DistanceMeters.Value);
                if (request.StrokeType is not null) swimming.SetStrokeType(Parse<SwimmingStroke>(request.StrokeType));
                break;
            case YogaUserWorkoutDetails yoga:
                if (request.Style is not null || request.Intensity is not null || request.FocusArea is not null)
                    yoga.SetDetails(Parse<YogaStyle>(request.Style) ?? yoga.Style, Parse<YogaIntensity>(request.Intensity) ?? yoga.Intensity, Parse<YogaFocusArea>(request.FocusArea) ?? yoga.FocusArea);
                break;
        }
    }

    private static TEnum? Parse<TEnum>(string? value) where TEnum : struct, Enum =>
        string.IsNullOrWhiteSpace(value) ? null : Enum.TryParse<TEnum>(value, true, out var parsed) ? parsed : throw new DomainException($"Invalid {typeof(TEnum).Name} value.");
}
