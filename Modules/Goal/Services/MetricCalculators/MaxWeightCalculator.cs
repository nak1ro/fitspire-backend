using backend.Modules.Workout.Domain.Entities;

namespace backend.Modules.Goal.Services.MetricCalculators;

public class MaxWeightCalculator : IExerciseMetricCalculator
{
    public string MetricName => "weight";

    public double Calculate(GymWorkoutExercise exercise)
    {
        return exercise.Weight;
    }
}
