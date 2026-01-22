using backend.Modules.Workout.Domain.Events;

namespace backend.Modules.Goal.Services.MetricCalculators;

public class DistanceMetricCalculator : IMetricCalculator
{
    public string MetricName => "distance";

    public double Calculate(WorkoutCompletedEvent workout)
    {
        return workout.DistanceKm ?? 0;
    }
}
