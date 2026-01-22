using MediatR;

namespace backend.Modules.Workout.Features.RunningWorkout;

public record CreateRunningWorkoutCommand(
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
) : IRequest<Guid>;
