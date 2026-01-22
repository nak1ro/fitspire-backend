using backend.Modules.Workout.Domain.Events;

namespace backend.Modules.Goal.Services.GoalProcessors;

public interface IWorkoutGoalProcessor
{
    string SupportedWorkoutType { get; }
    Task ProcessAsync(WorkoutCompletedEvent workout, CancellationToken cancellationToken);
}
