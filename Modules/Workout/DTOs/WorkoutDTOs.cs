using System.Text.Json;

namespace backend.Modules.Workout.DTOs;

// Request DTOs
public record CreateGymWorkoutRequest(
    DateTime Date,
    string? SplitType, // Mapped to WorkoutSplit enum
    string? IntensityLevel, // Mapped to WorkoutIntensity enum
    List<ExerciseInputRequest> Exercises
);

public record ExerciseInputRequest(
    Guid ExerciseId,
    List<GymSetInputRequest> Sets,
    string? Notes = null
);

public record GymSetInputRequest(
    int? Reps,
    double? WeightKg,
    int? DurationSeconds,
    double? DistanceMeters,
    bool IsWarmup,
    double? Rpe,
    string? Notes,
    bool IsCompleted = false);

public record AddGymExerciseRequest(Guid ExerciseId, List<GymSetInputRequest>? Sets = null, string? Notes = null);
public record UpdateGymExerciseRequest(string? Notes);
public record ReorderGymItemsRequest(IReadOnlyList<Guid> OrderedIds);
public record UpdateGymSetRequest(int? Reps, double? WeightKg, int? DurationSeconds, double? DistanceMeters, bool IsWarmup, double? Rpe, string? Notes);
public record SetCompletionRequest(bool IsCompleted);

public record CompleteWorkoutRequest(
    double? DurationMinutes,
    string? Notes,
    bool? IsPrivate
);

public record WorkoutSessionResponse(
    Guid Id,
    string WorkoutType,
    string Status,
    DateTime? StartedAt,
    DateTime? PausedAt,
    int AccumulatedPausedSeconds,
    double ElapsedMinutes
);

public record WorkoutFilterRequest(
    DateTime? From,
    DateTime? To,
    List<string>? Types
);

public record UpdateWorkoutRequest(
    DateTime? Date,
    double? DurationMinutes,
    string? Notes,
    bool? IsPrivate,
    int? CaloriesBurned = null,
    double? DistanceKm = null,
    double? ElevationGainMeters = null,
    int? StepCount = null,
    string? MapData = null,
    bool? IsIndoor = null,
    int? Laps = null,
    double? PoolLengthMeters = null,
    double? DistanceMeters = null,
    string? StrokeType = null,
    string? Style = null,
    string? Intensity = null,
    string? FocusArea = null,
    string? SplitType = null,
    string? IntensityLevel = null,
    List<ExerciseInputRequest>? Exercises = null
);

public record SaveRoutineRequest(
    string Name,
    string? Description
);

public record CreateFromRoutineRequest(
    DateTime Date
);

public record UpdateRoutineRequest(string Name, string? Description, JsonElement Definition);
public record WorkoutPageResponse(IReadOnlyList<WorkoutHistoryItemResponse> Items, int Page, int PageSize, int TotalCount);
public record ActivitySummaryResponse(DateTime From, DateTime To, int WorkoutCount, double DurationMinutes, double DistanceKm, double CaloriesBurned, double GymVolumeKg);

// Response DTOs
public record WorkoutResponse(
    Guid Id,
    Guid UserId,
    string WorkoutType,
    DateTime Date,
    double? DurationMinutes,
    string? Notes,
    bool IsPrivate,
    string Status,
    DateTime? CompletedAt,
    bool IsRoutine,
    string? RoutineName,
    DateTime CreatedAt
);

public record GymWorkoutResponse(
    Guid Id,
    Guid UserId,
    string WorkoutType,
    DateTime Date,
    double? DurationMinutes,
    string? Notes,
    bool IsPrivate,
    string Status,
    DateTime? CompletedAt,
    bool IsRoutine,
    string? RoutineName,
    string? SplitType,
    string? IntensityLevel,
    List<GymExerciseResponse> Exercises,
    DateTime CreatedAt
);

public record GymExerciseResponse(
    Guid Id,
    Guid ExerciseId,
    string ExerciseName,
    int OrderIndex,
    string? Notes,
    IReadOnlyList<GymSetResponse> Sets
);

public record GymSetResponse(
    Guid Id,
    int OrderIndex,
    int? Reps,
    double? WeightKg,
    int? DurationSeconds,
    double? DistanceMeters,
    bool IsWarmup,
    double? Rpe,
    string? Notes,
    bool IsCompleted,
    DateTime? CompletedAtUtc);

public record ExerciseCategoryResponse(
    Guid Id,
    string Name,
    string? Description,
    int ExercisesCount
);

public record ExerciseResponse(
    Guid Id,
    string Name,
    string? Description,
    Guid? CategoryId,
    string? CategoryName
);

public record WorkoutRoutineResponse(
    Guid Id,
    string Name,
    string? Description,
    string WorkoutType,
    int SchemaVersion,
    JsonElement Definition,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record PersonalRecordResponse(
    Guid Id,
    string WorkoutType,
    string Metric,
    string Unit,
    double Value,
    Guid WorkoutId,
    Guid? ExerciseId,
    string? ExerciseName,
    DateTime AchievedAt
);

public record WorkoutHistoryItemResponse(
    Guid Id, string WorkoutType, DateTime Date, double? DurationMinutes, int? CaloriesBurned,
    bool IsPrivate, string Status, DateTime? CompletedAt, Guid? CreatedFromRoutineId, string? NotesPreview,
    WorkoutHistorySummaryResponse Summary);

public record WorkoutHistorySummaryResponse(
    int? ExerciseCount = null, int? CompletedSetCount = null, double? TotalVolumeKg = null, double? MaximumWeightKg = null,
    double? DistanceKm = null, double? ElevationGainMeters = null, int? StepCount = null, double? AveragePaceMinutesPerKm = null,
    bool? IsIndoor = null, double? AverageSpeedKph = null, double? DistanceMeters = null, int? Laps = null,
    double? PoolLengthMeters = null, string? StrokeType = null, string? Style = null, string? Intensity = null, string? FocusArea = null);

public record CreateRunningWorkoutRequest(
    DateTime Date,
    double DistanceKm,
    double? DurationMinutes,
    double? ElevationGainMeters,
    int? StepCount,
    int? CaloriesBurned,
    string? MapData,
    string? Notes,
    bool IsPrivate
);

public record RunningWorkoutResponse(
    Guid Id,
    Guid UserId,
    string WorkoutType,
    DateTime Date,
    double? DurationMinutes,
    int? CaloriesBurned,
    string? Notes,
    bool IsPrivate,
    string Status,
    DateTime? CompletedAt,
    double DistanceKm,
    double? ElevationGainMeters,
    int? StepCount,
    string? MapData,
    DateTime CreatedAt
);

public record CreateCyclingWorkoutRequest(
    DateTime Date,
    double DistanceKm,
    double? DurationMinutes,
    double? ElevationGainMeters,
    int? CaloriesBurned,
    string? MapData,
    string? Notes,
    bool IsPrivate,
    bool IsIndoor
);

public record CyclingWorkoutResponse(
    Guid Id,
    Guid UserId,
    string WorkoutType,
    DateTime Date,
    double? DurationMinutes,
    int? CaloriesBurned,
    string? Notes,
    bool IsPrivate,
    string Status,
    DateTime? CompletedAt,
    double DistanceKm,
    double? ElevationGainMeters,
    string? MapData,
    bool IsIndoor,
    DateTime CreatedAt
);

public record CreateSwimmingWorkoutRequest(
    DateTime Date,
    int? Laps,
    double? PoolLengthMeters,
    double? DistanceMeters,
    string? StrokeType, // Mapped to SwimmingStroke enum
    double? DurationMinutes,
    int? CaloriesBurned,
    string? Notes,
    bool IsPrivate
);

public record SwimmingWorkoutResponse(
    Guid Id,
    Guid UserId,
    string WorkoutType,
    DateTime Date,
    double? DurationMinutes,
    int? CaloriesBurned,
    string? Notes,
    bool IsPrivate,
    string Status,
    DateTime? CompletedAt,
    int? Laps,
    double? PoolLengthMeters,
    double? DistanceMeters,
    string? StrokeType,
    DateTime CreatedAt
);

public record CreateYogaWorkoutRequest(
    DateTime Date,
    double? DurationMinutes,
    string? Style, // Mapped to Enum
    string? Intensity, // Mapped to Enum
    string? FocusArea, // Mapped to Enum
    int? CaloriesBurned,
    string? Notes,
    bool IsPrivate
);

public record YogaWorkoutResponse(
    Guid Id,
    Guid UserId,
    string WorkoutType,
    DateTime Date,
    double? DurationMinutes,
    int? CaloriesBurned,
    string? Notes,
    bool IsPrivate,
    string Status,
    DateTime? CompletedAt,
    string? Style,
    string? Intensity,
    string? FocusArea,
    DateTime CreatedAt
);
