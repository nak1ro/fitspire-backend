using backend.Modules.Workout.Domain.Events;

namespace backend.Modules.Goal.Services.MetricCalculators;

public class CountMetricCalculator : IMetricCalculator
{
    public string MetricName => "count";

    public double Calculate(WorkoutCompletedEvent workout)
    {
        return 1;
    }
}
