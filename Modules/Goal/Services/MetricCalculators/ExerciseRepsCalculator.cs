using backend.Modules.Workout.Domain.Entities;

namespace backend.Modules.Goal.Services.MetricCalculators;

public class ExerciseRepsCalculator : IExerciseMetricCalculator
{
    public string MetricName => "reps";

    public double Calculate(GymWorkoutExercise exercise)
    {
        return exercise.Sets * exercise.Reps;
    }
}
