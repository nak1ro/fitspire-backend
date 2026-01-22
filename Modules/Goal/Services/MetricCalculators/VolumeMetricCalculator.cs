using backend.Modules.Workout.Domain.Events;

namespace backend.Modules.Goal.Services.MetricCalculators;

public class VolumeMetricCalculator : IMetricCalculator
{
    public string MetricName => "volume";

    public double Calculate(WorkoutCompletedEvent workout)
    {
        return workout.TotalVolumeKg ?? 0;
    }
}
