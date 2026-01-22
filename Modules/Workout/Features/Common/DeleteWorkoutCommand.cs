using MediatR;

namespace backend.Modules.Workout.Features.Common;

public record DeleteWorkoutCommand(Guid WorkoutId, Guid UserId) : IRequest;
