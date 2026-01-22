using MediatR;

namespace backend.Modules.Workout.Features.Common;

public record CompleteWorkoutCommand(
    Guid WorkoutId,
    double? DurationMinutes
) : IRequest<bool>;
