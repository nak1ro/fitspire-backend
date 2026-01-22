using backend.Modules.Workout.Domain.Entities;

namespace backend.Modules.Goal.Services.MetricCalculators;

public interface IExerciseMetricCalculator
{
    string MetricName { get; }
    double Calculate(GymWorkoutExercise exercise);
}
