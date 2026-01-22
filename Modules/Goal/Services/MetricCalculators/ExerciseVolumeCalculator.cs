using backend.Modules.Workout.Domain.Entities;

namespace backend.Modules.Goal.Services.MetricCalculators;

public class ExerciseVolumeCalculator : IExerciseMetricCalculator
{
    public string MetricName => "volume";

    public double Calculate(GymWorkoutExercise exercise)
    {
        return exercise.CalculateVolume();
    }
}
