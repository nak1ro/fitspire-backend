using backend.Data;
using backend.Modules.Goal.Domain.Entities;
using backend.Modules.Goal.Domain.Enums;
using backend.Modules.Progress.Services;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.Goal.Data;

public class GoalTypeSeeder
{
    private static readonly string[] RetiredCodes =
    [
        "workout-calories",
        "distance",
        "single-distance",
        "exercise-max-weight"
    ];

    private readonly FitspireDbContext _context;

    public GoalTypeSeeder(FitspireDbContext context) => _context = context;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var templates = CreateTemplates();
        for (var index = 0; index < templates.Length; index++)
            await SynchronizeAsync(templates[index], index + 1, cancellationToken);

        var retired = await _context.GoalTypes.Where(type => RetiredCodes.Contains(type.Code)).ToListAsync(cancellationToken);
        foreach (var template in retired)
            template.Retire();

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task SynchronizeAsync(GoalTemplateSeed seed, int displayOrder, CancellationToken cancellationToken)
    {
        var existing = await _context.GoalTypes.SingleOrDefaultAsync(type => type.Code == seed.Code, cancellationToken);
        if (existing is null)
        {
            await _context.GoalTypes.AddAsync(new GoalType(Guid.NewGuid(), seed.Name, seed.Unit, GoalCategory.Fitness,
                seed.MeasurementType, seed.Description, null, seed.WorkoutType, null, null, seed.Code,
                seed.MetricCode, seed.ParameterKind, displayOrder), cancellationToken);
            return;
        }

        existing.Synchronize(seed.Name, seed.Unit, GoalCategory.Fitness, seed.MeasurementType, seed.Description,
            null, seed.WorkoutType, null, null, seed.MetricCode, seed.ParameterKind, displayOrder);
    }

    private static GoalTemplateSeed[] CreateTemplates() =>
    [
        new("workout-count", "Complete workouts", "workouts", MetricCatalogue.WorkoutCount, GoalMeasurementType.Cumulative, null, "WorkoutType"),
        new("workout-streak", "Workout streak", "days", MetricCatalogue.WorkoutCount, GoalMeasurementType.Streak, null, "None"),
        new("workout-duration", "Workout minutes", "minutes", MetricCatalogue.DurationMinutes, GoalMeasurementType.Cumulative, null, "WorkoutType"),
        new("gym-volume", "Total lifting volume", "kg", MetricCatalogue.GymVolumeKg, GoalMeasurementType.Cumulative, "gym", "None"),
        new("workout-calories-canonical", "Burn calories", "kcal", MetricCatalogue.CaloriesKcal, GoalMeasurementType.Cumulative, null, "WorkoutType"),
        new("running-distance", "Running distance", "km", MetricCatalogue.RunningDistanceKm, GoalMeasurementType.Cumulative, "running", "None"),
        new("cycling-distance", "Cycling distance", "km", MetricCatalogue.CyclingDistanceKm, GoalMeasurementType.Cumulative, "cycling", "None"),
        new("swimming-distance", "Swimming distance", "m", MetricCatalogue.SwimmingDistanceMeters, GoalMeasurementType.Cumulative, "swimming", "None"),
        new("yoga-duration", "Yoga minutes", "minutes", MetricCatalogue.YogaDurationMinutes, GoalMeasurementType.Cumulative, "yoga", "None"),
        new("gym-exercise-count", "Gym exercises", "exercises", MetricCatalogue.GymExerciseCount, GoalMeasurementType.Cumulative, "gym", "None"),
        new("running-single-distance", "Single running distance", "km", MetricCatalogue.RunningDistanceKm, GoalMeasurementType.SingleEvent, "running", "None"),
        new("cycling-single-distance", "Single cycling distance", "km", MetricCatalogue.CyclingDistanceKm, GoalMeasurementType.SingleEvent, "cycling", "None"),
        new("swimming-single-distance", "Single swimming distance", "m", MetricCatalogue.SwimmingDistanceMeters, GoalMeasurementType.SingleEvent, "swimming", "None"),
        new("exercise-max-weight-canonical", "Exercise max weight", "kg", MetricCatalogue.ExerciseMaxWeightKg, GoalMeasurementType.SingleEvent, "gym", "Exercise")
    ];

    private sealed record GoalTemplateSeed(
        string Code,
        string Name,
        string Unit,
        string MetricCode,
        GoalMeasurementType MeasurementType,
        string? WorkoutType,
        string ParameterKind)
    {
        public string Description => $"Track {Name.ToLowerInvariant()} from completed workouts.";
    }
}
