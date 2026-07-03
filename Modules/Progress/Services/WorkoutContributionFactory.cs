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
        AddIfPositive(contributions, workout, MetricCatalogue.LegacyCalories, workout.CaloriesBurned);
        AddIfPositive(contributions, workout, MetricCatalogue.CaloriesKcal, workout.CaloriesBurned);
        AddIfPositive(contributions, workout, MetricCatalogue.LegacyDistanceKm, workout.GetTotalDistance());

        switch (workout)
        {
            case RunningUserWorkoutDetails:
                AddIfPositive(contributions, workout, MetricCatalogue.RunningDistanceKm, workout.GetTotalDistance());
                break;
            case CyclingUserWorkoutDetails:
                AddIfPositive(contributions, workout, MetricCatalogue.CyclingDistanceKm, workout.GetTotalDistance());
                break;
            case SwimmingUserWorkoutDetails swimming:
                AddIfPositive(contributions, workout, MetricCatalogue.SwimmingDistanceMeters, swimming.DistanceMeters);
                break;
            case YogaUserWorkoutDetails:
                AddIfPositive(contributions, workout, MetricCatalogue.YogaDurationMinutes, workout.DurationMinutes);
                break;
        }

        if (workout is GymUserWorkoutDetails gym)
        {
            AddIfPositive(contributions, workout, MetricCatalogue.GymVolumeKg, gym.GetTotalVolume());
            AddIfPositive(contributions, workout, MetricCatalogue.GymExerciseCount, gym.GetExerciseCount());
            foreach (var exercise in gym.Exercises)
            {
                var maximumWeight = exercise.GetMaximumCompletedWeight();
                AddIfPositive(contributions, workout, MetricCatalogue.LegacyGymMaxWeightKg, maximumWeight, exercise.ExerciseId);
                AddIfPositive(contributions, workout, MetricCatalogue.ExerciseMaxWeightKg, maximumWeight, exercise.ExerciseId);
            }
        }

        return contributions;
    }

    private static void AddIfPositive(List<ActivityContribution> contributions, UserWorkout workout, string metricCode, double? value, Guid? exerciseId = null)
    {
        if (value is > 0)
            contributions.Add(new ActivityContribution(workout.UserId, workout.Id, metricCode, value.Value, workout.WorkoutType, workout.Date, exerciseId));
    }
}
