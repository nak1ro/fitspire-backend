using MediatR;

namespace backend.Modules.Workout.Features.SwimmingWorkout;

public record CreateSwimmingWorkoutCommand(
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
) : IRequest<Guid>;
