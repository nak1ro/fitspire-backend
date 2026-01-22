using backend.Modules.Workout.Domain.Enums;
using MediatR;

namespace backend.Modules.Workout.Features.YogaWorkout;

public record CreateYogaWorkoutCommand(
    Guid UserId,
    DateTime Date,
    YogaStyle? Style,
    YogaIntensity? Intensity,
    YogaFocusArea? FocusArea,
    double? DurationMinutes,
    int? CaloriesBurned,
    string? Notes,
    bool IsPrivate
) : IRequest<Guid>;
