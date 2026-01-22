using backend.Modules.Workout.Domain;
using MediatR;

namespace backend.Modules.Workout.Queries;

public record GetWorkoutByIdQuery(Guid WorkoutId) : IRequest<UserWorkout?>;
