using backend.Data;
using backend.Modules.Goal.Domain.Entities;
using backend.Modules.Goal.Domain.Enums;
using backend.Modules.Progress.Services;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.Goal.Data;

public class GoalTypeSeeder
{
    private readonly FitspireDbContext _context;

    public GoalTypeSeeder(FitspireDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        var definitions = new[]
        {
            ("workout-count", "Complete workouts", "workouts", GoalMeasurementType.Cumulative, MetricCatalogue.WorkoutCount, "WorkoutType"),
            ("workout-duration", "Workout minutes", "minutes", GoalMeasurementType.Cumulative, MetricCatalogue.DurationMinutes, "WorkoutType"),
            ("workout-calories", "Burn calories", "kcal", GoalMeasurementType.Cumulative, MetricCatalogue.Calories, "WorkoutType"),
            ("distance", "Cover distance", "km", GoalMeasurementType.Cumulative, MetricCatalogue.DistanceKm, "WorkoutType"),
            ("single-distance", "Single workout distance", "km", GoalMeasurementType.SingleEvent, MetricCatalogue.DistanceKm, "WorkoutType"),
            ("gym-volume", "Total lifting volume", "kg", GoalMeasurementType.Cumulative, MetricCatalogue.GymVolumeKg, "None"),
            ("exercise-max-weight", "Exercise max weight", "kg", GoalMeasurementType.SingleEvent, MetricCatalogue.GymMaxWeightKg, "Exercise")
        };

        for (var index = 0; index < definitions.Length; index++)
        {
            var (code, name, unit, measurementType, metricCode, parameterKind) = definitions[index];
            if (await _context.GoalTypes.AnyAsync(type => type.Code == code))
                continue;

            await _context.GoalTypes.AddAsync(new GoalType(Guid.NewGuid(), name, unit, GoalCategory.Fitness, measurementType,
                $"Track {name.ToLowerInvariant()} from completed workouts.", null, null, null, null,
                code, metricCode, parameterKind, index + 1));
        }
        await _context.SaveChangesAsync();
    }
}
