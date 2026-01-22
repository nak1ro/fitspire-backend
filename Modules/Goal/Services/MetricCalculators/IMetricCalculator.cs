using backend.Modules.Workout.Domain.Events;

namespace backend.Modules.Goal.Services.MetricCalculators;

public interface IMetricCalculator
{
    string MetricName { get; }
    double Calculate(WorkoutCompletedEvent workout);
}
