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
