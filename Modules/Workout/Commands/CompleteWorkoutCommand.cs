using MediatR;

namespace backend.Modules.Workout.Commands;

public record CompleteWorkoutCommand(
    Guid WorkoutId,
    double? DurationMinutes
) : IRequest<bool>;
