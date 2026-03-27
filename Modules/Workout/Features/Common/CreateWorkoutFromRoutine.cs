using System.Text.Json;
using backend.Modules.Shared;
using backend.Modules.Shared.Domain;
using backend.Modules.Workout.Domain.Entities;
using backend.Modules.Workout.Domain.Enums;
using backend.Modules.Workout.Infrastructure;
using MediatR;

namespace backend.Modules.Workout.Features.Common;

public record CreateWorkoutFromRoutineCommand(
    Guid CurrentUserId,
    Guid RoutineId,
    DateTime Date
) : IRequest<Guid>;

public class CreateWorkoutFromRoutineHandler : IRequestHandler<CreateWorkoutFromRoutineCommand, Guid>
{
    private readonly IWorkoutRepository _workoutRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateWorkoutFromRoutineHandler(IWorkoutRepository workoutRepository, IUnitOfWork unitOfWork)
    {
        _workoutRepository = workoutRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateWorkoutFromRoutineCommand request, CancellationToken cancellationToken)
    {
        var routine = await _workoutRepository.GetRoutineByIdAsync(request.RoutineId, cancellationToken);
        if (routine == null)
            throw new NotFoundException($"Routine {request.RoutineId} not found.");

        if (routine.UserId != request.CurrentUserId)
            throw new UnauthorizedAccessException("Cannot use another user's routine.");

        var newWorkout = CreateWorkoutFromRoutineData(routine, request.CurrentUserId, request.Date);
        newWorkout.SetCreatedFromRoutine(routine.Id);

        await _workoutRepository.AddAsync(newWorkout, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return newWorkout.Id;
    }

    private static UserWorkout CreateWorkoutFromRoutineData(WorkoutRoutine routine, Guid userId, DateTime date)
    {
        using var document = JsonDocument.Parse(routine.RoutineDataJson);
        var root = document.RootElement;

        UserWorkout workout = routine.WorkoutType switch
        {
            "gym" => CreateGymWorkout(root, userId, date),
            "running" => CreateRunningWorkout(root, userId, date),
            "cycling" => CreateCyclingWorkout(root, userId, date),
            "swimming" => CreateSwimmingWorkout(root, userId, date),
            "yoga" => CreateYogaWorkout(root, userId, date),
            _ => throw new DomainException($"Unsupported routine type: {routine.WorkoutType}")
        };

        ApplyCommonRoutineDetails(workout, root, date);
        return workout;
    }

    private static GymUserWorkoutDetails CreateGymWorkout(JsonElement root, Guid userId, DateTime date)
    {
        var workout = new GymUserWorkoutDetails(
            Guid.NewGuid(),
            userId,
            date,
            GetEnum<WorkoutSplit>(root, "SplitType"));

        var intensity = GetEnum<WorkoutIntensity>(root, "IntensityLevel");
        if (intensity.HasValue)
            workout.SetIntensity(intensity.Value);

        if (root.TryGetProperty("Exercises", out var exercises) && exercises.ValueKind == JsonValueKind.Array)
        {
            foreach (var exercise in exercises.EnumerateArray())
            {
                workout.AddExercise(
                    GetRequiredGuid(exercise, "ExerciseId"),
                    GetRequiredInt(exercise, "Sets"),
                    GetRequiredInt(exercise, "Reps"),
                    GetRequiredDouble(exercise, "Weight"));
            }
        }

        return workout;
    }

    private static RunningUserWorkoutDetails CreateRunningWorkout(JsonElement root, Guid userId, DateTime date)
    {
        var workout = new RunningUserWorkoutDetails(
            Guid.NewGuid(),
            userId,
            date,
            GetRequiredDouble(root, "DistanceKm"),
            null);

        workout.SetStats(GetDouble(root, "ElevationGainMeters"), GetInt(root, "StepCount"), GetString(root, "MapData"));
        return workout;
    }

    private static CyclingUserWorkoutDetails CreateCyclingWorkout(JsonElement root, Guid userId, DateTime date)
    {
        var workout = new CyclingUserWorkoutDetails(
            Guid.NewGuid(),
            userId,
            date,
            GetRequiredDouble(root, "DistanceKm"),
            null,
            GetBool(root, "IsIndoor") ?? false);

        workout.UpdateStats(GetDouble(root, "ElevationGainMeters"), GetString(root, "MapData"));
        return workout;
    }

    private static SwimmingUserWorkoutDetails CreateSwimmingWorkout(JsonElement root, Guid userId, DateTime date)
    {
        var workout = new SwimmingUserWorkoutDetails(Guid.NewGuid(), userId, date, null);

        workout.SetPoolDetails(GetInt(root, "Laps"), GetDouble(root, "PoolLengthMeters"));
        if (GetDouble(root, "DistanceMeters") is { } distanceMeters)
            workout.SetDistance(distanceMeters);

        if (GetEnum<SwimmingStroke>(root, "StrokeType") is { } stroke)
            workout.SetStrokeType(stroke);

        return workout;
    }

    private static YogaUserWorkoutDetails CreateYogaWorkout(JsonElement root, Guid userId, DateTime date)
    {
        var workout = new YogaUserWorkoutDetails(Guid.NewGuid(), userId, date, null, null);
        workout.SetDetails(
            GetEnum<YogaStyle>(root, "Style"),
            GetEnum<YogaIntensity>(root, "Intensity"),
            GetEnum<YogaFocusArea>(root, "FocusArea"));

        return workout;
    }

    private static void ApplyCommonRoutineDetails(UserWorkout workout, JsonElement root, DateTime date)
    {
        workout.UpdateDetails(date, GetDouble(root, "DurationMinutes"), GetString(root, "Notes"), GetBool(root, "IsPrivate"));
        workout.SetCalories(GetInt(root, "CaloriesBurned"));
    }

    private static string? GetString(JsonElement root, string propertyName)
    {
        return root.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;
    }

    private static int? GetInt(JsonElement root, string propertyName)
    {
        return root.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number
            ? property.GetInt32()
            : null;
    }

    private static int GetRequiredInt(JsonElement root, string propertyName)
    {
        return GetInt(root, propertyName)
            ?? throw new DomainException($"Routine data is missing {propertyName}.");
    }

    private static double? GetDouble(JsonElement root, string propertyName)
    {
        return root.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number
            ? property.GetDouble()
            : null;
    }

    private static double GetRequiredDouble(JsonElement root, string propertyName)
    {
        return GetDouble(root, propertyName)
            ?? throw new DomainException($"Routine data is missing {propertyName}.");
    }

    private static bool? GetBool(JsonElement root, string propertyName)
    {
        return root.TryGetProperty(propertyName, out var property) && property.ValueKind is JsonValueKind.True or JsonValueKind.False
            ? property.GetBoolean()
            : null;
    }

    private static Guid GetRequiredGuid(JsonElement root, string propertyName)
    {
        return root.TryGetProperty(propertyName, out var property) && property.TryGetGuid(out var value)
            ? value
            : throw new DomainException($"Routine data is missing {propertyName}.");
    }

    private static TEnum? GetEnum<TEnum>(JsonElement root, string propertyName)
        where TEnum : struct, Enum
    {
        if (!root.TryGetProperty(propertyName, out var property) || property.ValueKind == JsonValueKind.Null)
            return null;

        if (property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var number))
            return (TEnum)Enum.ToObject(typeof(TEnum), number);

        if (property.ValueKind == JsonValueKind.String && Enum.TryParse<TEnum>(property.GetString(), true, out var parsed))
            return parsed;

        return null;
    }
}
