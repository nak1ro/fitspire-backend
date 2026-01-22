using backend.Modules.Workout.Domain.Entities;

namespace backend.Modules.Goal.Services.MetricCalculators;

public class ExerciseCountCalculator : IExerciseMetricCalculator
{
    public string MetricName => "count";

    public double Calculate(GymWorkoutExercise exercise)
    {
        return 1;
    }
}
