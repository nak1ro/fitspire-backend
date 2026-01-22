using MediatR;

namespace backend.Modules.Workout.Features.CyclingWorkout;

public record CreateCyclingWorkoutCommand(
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
) : IRequest<Guid>;
