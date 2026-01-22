using backend.Modules.Workout.Domain.Events;

namespace backend.Modules.Goal.Services.MetricCalculators;

public class DurationMetricCalculator : IMetricCalculator
{
    public string MetricName => "duration";

    public double Calculate(WorkoutCompletedEvent workout)
    {
        return workout.DurationMinutes ?? 0;
    }
}
