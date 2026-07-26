using backend.Modules.Shared;
using backend.Modules.Shared.Domain;
using backend.Modules.Workout.Domain.Entities;
using backend.Modules.Workout.Domain.Enums;
using backend.Modules.Workout.Infrastructure;
using backend.Modules.Workout.Services;
using MediatR;

namespace backend.Modules.Workout.Features.GymWorkout;

public record CreateGymWorkoutCommand(
    Guid UserId,
    DateTime Date,
    string? SplitType,
    string? IntensityLevel,
    List<ExerciseInput> Exercises
) : IRequest<Guid>;

public record ExerciseInput(
    Guid ExerciseId,
    IReadOnlyList<SetInput> Sets,
    string? Notes
);

public record SetInput(
    int? Reps,
    double? WeightKg,
    int? DurationSeconds,
    double? DistanceMeters,
    bool IsWarmup,
    double? Rpe,
    string? Notes,
    bool IsCompleted);

public class CreateGymWorkoutHandler : IRequestHandler<CreateGymWorkoutCommand, Guid>
{
    private readonly IWorkoutRepository _workoutRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWorkoutOccurrenceTimeService _occurrenceTimeService;
    private readonly IWorkoutSessionGuard _sessionGuard;

    public CreateGymWorkoutHandler(
        IWorkoutRepository workoutRepository,
        IUnitOfWork unitOfWork,
        IWorkoutOccurrenceTimeService occurrenceTimeService,
        IWorkoutSessionGuard sessionGuard)
    {
        _workoutRepository = workoutRepository;
        _unitOfWork = unitOfWork;
        _occurrenceTimeService = occurrenceTimeService;
        _sessionGuard = sessionGuard;
    }

    public async Task<Guid> Handle(CreateGymWorkoutCommand request, CancellationToken cancellationToken)
    {
        await _sessionGuard.EnsureCanStartAsync(request.UserId, cancellationToken);
        await EnsureExercisesExistAsync(request.Exercises, cancellationToken);
        var occurredAtUtc = await _occurrenceTimeService.ResolveUtcAsync(request.UserId, request.Date, cancellationToken);
        var workout = new GymUserWorkoutDetails(
            Guid.NewGuid(),
            request.UserId,
            occurredAtUtc,
            Enum.TryParse<WorkoutSplit>(request.SplitType, true, out var split) ? split : null
        );

        if (Enum.TryParse<WorkoutIntensity>(request.IntensityLevel, true, out var intensity))
            workout.SetIntensity(intensity);

        foreach (var exercise in request.Exercises)
        {
            var entry = workout.AddExercise(exercise.ExerciseId, exercise.Notes);
            foreach (var set in exercise.Sets)
                entry.AddSet(set.Reps, set.WeightKg, set.DurationSeconds, set.DistanceMeters,
                    set.IsWarmup, set.Rpe, set.Notes, set.IsCompleted);
        }

        await _workoutRepository.AddAsync(workout, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return workout.Id;
    }

    private async Task EnsureExercisesExistAsync(IReadOnlyCollection<ExerciseInput> exercises, CancellationToken cancellationToken)
    {
        var requestedIds = exercises.Select(exercise => exercise.ExerciseId).Distinct().ToHashSet();
        var existingIds = (await _workoutRepository.GetExercisesAsync(null, null, cancellationToken))
            .Select(exercise => exercise.Id)
            .ToHashSet();

        if (!requestedIds.IsSubsetOf(existingIds))
            throw new DomainException("One or more exercises do not exist.");
    }
}
