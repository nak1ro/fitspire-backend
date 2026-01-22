namespace backend.Modules.Workout.DTOs;

// Request DTOs
public record CreateGymWorkoutRequest(
    Guid UserId,
    DateTime Date,
    string? SplitType,
    string? IntensityLevel,
    List<ExerciseInputRequest> Exercises
);

public record ExerciseInputRequest(
    Guid ExerciseId,
    int Sets,
    int Reps,
    double WeightKg
);

public record CompleteWorkoutRequest(
    double? DurationMinutes
);

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
    int Sets,
    int Reps,
    double Weight,
    int OrderIndex
);

public record CreateRunningWorkoutRequest(
    Guid UserId,
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
    Guid UserId,
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
    Guid UserId,
    DateTime Date,
    int? Laps,
    double? PoolLengthMeters,
    double? DistanceMeters,
    string? StrokeType,
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
