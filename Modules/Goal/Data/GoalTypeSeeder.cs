using backend.Data;
using backend.Modules.Goal.Domain.Entities;
using backend.Modules.Goal.Domain.Enums;
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
        if (await _context.GoalTypes.AnyAsync())
            return;

        var goalTypes = new List<GoalType>
        {
            // === FITNESS GOALS ===
            
            // Gym
            new GoalType(Guid.NewGuid(), "Complete Gym Workouts", "workouts", GoalCategory.Fitness, GoalMeasurementType.Cumulative,
                "Complete a target number of gym workouts", null, "gym", "count"),
            new GoalType(Guid.NewGuid(), "Bench Press Target", "kg", GoalCategory.Fitness, GoalMeasurementType.SingleEvent,
                "Achieve a bench press weight goal", null, "gym", "weight"),
            new GoalType(Guid.NewGuid(), "Squat Target", "kg", GoalCategory.Fitness, GoalMeasurementType.SingleEvent,
                "Achieve a squat weight goal", null, "gym", "weight"),
            new GoalType(Guid.NewGuid(), "Deadlift Target", "kg", GoalCategory.Fitness, GoalMeasurementType.SingleEvent,
                "Achieve a deadlift weight goal", null, "gym", "weight"),
            new GoalType(Guid.NewGuid(), "Total Lifting Volume", "kg", GoalCategory.Fitness, GoalMeasurementType.Cumulative,
                "Lift a total amount of weight over time", null, "gym", "volume"),

            // Running
            new GoalType(Guid.NewGuid(), "Run Total Distance", "km", GoalCategory.Fitness, GoalMeasurementType.Cumulative,
                "Run a total distance over a period", null, "running", "distance"),
            new GoalType(Guid.NewGuid(), "Run Single Distance", "km", GoalCategory.Fitness, GoalMeasurementType.SingleEvent,
                "Complete a single run of target distance", null, "running", "distance"),
            new GoalType(Guid.NewGuid(), "Complete Running Sessions", "sessions", GoalCategory.Fitness, GoalMeasurementType.Cumulative,
                "Complete a number of running sessions", null, "running", "count"),

            // Cycling
            new GoalType(Guid.NewGuid(), "Cycle Total Distance", "km", GoalCategory.Fitness, GoalMeasurementType.Cumulative,
                "Cycle a total distance over a period", null, "cycling", "distance"),
            new GoalType(Guid.NewGuid(), "Cycle Single Distance", "km", GoalCategory.Fitness, GoalMeasurementType.SingleEvent,
                "Complete a single cycling session of target distance", null, "cycling", "distance"),
            new GoalType(Guid.NewGuid(), "Complete Cycling Sessions", "sessions", GoalCategory.Fitness, GoalMeasurementType.Cumulative,
                "Complete a number of cycling sessions", null, "cycling", "count"),

            // Swimming
            new GoalType(Guid.NewGuid(), "Swim Total Distance", "m", GoalCategory.Fitness, GoalMeasurementType.Cumulative,
                "Swim a total distance over a period", null, "swimming", "distance"),
            new GoalType(Guid.NewGuid(), "Swim Single Distance", "m", GoalCategory.Fitness, GoalMeasurementType.SingleEvent,
                "Complete a single swim of target distance", null, "swimming", "distance"),
            new GoalType(Guid.NewGuid(), "Complete Swimming Sessions", "sessions", GoalCategory.Fitness, GoalMeasurementType.Cumulative,
                "Complete a number of swimming sessions", null, "swimming", "count"),

            // Yoga
            new GoalType(Guid.NewGuid(), "Complete Yoga Sessions", "sessions", GoalCategory.Fitness, GoalMeasurementType.Cumulative,
                "Complete a number of yoga sessions", null, "yoga", "count"),
            new GoalType(Guid.NewGuid(), "Yoga Minutes", "minutes", GoalCategory.Fitness, GoalMeasurementType.Cumulative,
                "Accumulate yoga practice time", null, "yoga", "duration"),

            // General Fitness
            new GoalType(Guid.NewGuid(), "Workout Streak", "days", GoalCategory.Fitness, GoalMeasurementType.Streak,
                "Maintain a workout streak", null, "any", "count"),
            new GoalType(Guid.NewGuid(), "Complete Any Workouts", "workouts", GoalCategory.Fitness, GoalMeasurementType.Cumulative,
                "Complete workouts of any type", null, "any", "count"),
            new GoalType(Guid.NewGuid(), "Burn Calories", "kcal", GoalCategory.Fitness, GoalMeasurementType.Cumulative,
                "Burn a target number of calories from workouts", null, "any", "calories"),

            // === BODY GOALS ===
            new GoalType(Guid.NewGuid(), "Reach Target Weight", "kg", GoalCategory.Body, GoalMeasurementType.Threshold,
                "Reach a target body weight", null, null, "weight"),
            new GoalType(Guid.NewGuid(), "Reach Body Fat Percentage", "%", GoalCategory.Body, GoalMeasurementType.Threshold,
                "Reach a target body fat percentage", null, null, "bodyfat"),
            new GoalType(Guid.NewGuid(), "Gain Muscle Mass", "kg", GoalCategory.Body, GoalMeasurementType.Cumulative,
                "Gain a target amount of muscle", null, null, "muscle"),

            // === NUTRITION GOALS ===
            new GoalType(Guid.NewGuid(), "Daily Calorie Target", "kcal", GoalCategory.Nutrition, GoalMeasurementType.Threshold,
                "Hit a daily calorie target", null, null, "calories"),
            new GoalType(Guid.NewGuid(), "Daily Protein Target", "g", GoalCategory.Nutrition, GoalMeasurementType.Threshold,
                "Hit a daily protein target", null, null, "protein"),
            new GoalType(Guid.NewGuid(), "Log Meals Streak", "days", GoalCategory.Nutrition, GoalMeasurementType.Streak,
                "Log meals for consecutive days", null, null, "count"),

            // === HABIT GOALS ===
            new GoalType(Guid.NewGuid(), "Daily Water Intake", "liters", GoalCategory.Habit, GoalMeasurementType.Threshold,
                "Drink a target amount of water daily", null, null, "water"),
            new GoalType(Guid.NewGuid(), "Sleep Hours", "hours", GoalCategory.Habit, GoalMeasurementType.Threshold,
                "Get a target number of sleep hours", null, null, "sleep"),
        };

        await _context.GoalTypes.AddRangeAsync(goalTypes);
        await _context.SaveChangesAsync();
    }
}
