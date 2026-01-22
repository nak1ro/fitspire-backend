using backend.Modules.Workout.Domain;
using MediatR;

using backend.Modules.Workout.Domain.Entities;
using MediatR;

namespace backend.Modules.Workout.Features.Common;

public record GetWorkoutByIdQuery(Guid WorkoutId) : IRequest<UserWorkout?>;
