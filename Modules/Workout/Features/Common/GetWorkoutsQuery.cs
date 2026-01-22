using backend.Modules.Workout.DTOs;
using MediatR;

namespace backend.Modules.Workout.Features.Common;

public record GetWorkoutsQuery(Guid UserId, WorkoutFilterRequest Filter) : IRequest<List<WorkoutResponse>>;
