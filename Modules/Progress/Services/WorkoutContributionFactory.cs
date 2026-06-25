using backend.Modules.Progress.Domain;
using backend.Modules.Workout.Domain.Entities;

namespace backend.Modules.Progress.Services;

public static class WorkoutContributionFactory
{
    public static IReadOnlyList<ActivityContribution> Create(UserWorkout workout)
    {
        if (workout.CompletedAt is null || workout.DeletedAt is not null)
            return [];

        var contributions = new List<ActivityContribution>
        {
            new(workout.UserId, workout.Id, MetricCatalogue.WorkoutCount, 1, workout.WorkoutType, workout.Date)
        };

        AddIfPositive(contributions, workout, MetricCatalogue.DurationMinutes, workout.DurationMinutes);
        AddIfPositive(contributions, workout, MetricCatalogue.Calories, workout.CaloriesBurned);
        AddIfPositive(contributions, workout, MetricCatalogue.DistanceKm, workout.GetTotalDistance());

        if (workout is GymUserWorkoutDetails gym)
        {
            AddIfPositive(contributions, workout, MetricCatalogue.GymVolumeKg, gym.GetTotalVolume());
            foreach (var exercise in gym.Exercises)
                AddIfPositive(contributions, workout, MetricCatalogue.GymMaxWeightKg, exercise.Weight, exercise.ExerciseId);
        }

        return contributions;
    }

    private static void AddIfPositive(List<ActivityContribution> contributions, UserWorkout workout, string metricCode, double? value, Guid? exerciseId = null)
    {
        if (value is > 0)
            contributions.Add(new ActivityContribution(workout.UserId, workout.Id, metricCode, value.Value, workout.WorkoutType, workout.Date, exerciseId));
    }
}
