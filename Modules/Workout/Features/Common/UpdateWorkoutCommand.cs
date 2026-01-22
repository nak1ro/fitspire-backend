using backend.Modules.Workout.DTOs;
using MediatR;

namespace backend.Modules.Workout.Features.Common;

public record UpdateWorkoutCommand(Guid WorkoutId, Guid UserId, UpdateWorkoutRequest Request) : IRequest;
