using backend.Modules.Workout.Domain.Events;

namespace backend.Modules.Goal.Services.MetricCalculators;

public class CaloriesMetricCalculator : IMetricCalculator
{
    public string MetricName => "calories";

    public double Calculate(WorkoutCompletedEvent workout)
    {
        return workout.CaloriesBurned ?? 0;
    }
}
