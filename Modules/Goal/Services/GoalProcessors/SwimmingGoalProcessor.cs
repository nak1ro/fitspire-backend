using backend.Modules.Goal.Domain.Entities;
using backend.Modules.Goal.Infrastructure;
using backend.Modules.Workout.Domain.Events;

namespace backend.Modules.Goal.Services.GoalProcessors;

public class SwimmingGoalProcessor : IWorkoutGoalProcessor
{
    private readonly IGoalRepository _goalRepository;

    public string SupportedWorkoutType => "swimming";

    public SwimmingGoalProcessor(IGoalRepository goalRepository)
    {
        _goalRepository = goalRepository;
    }

    public async Task ProcessAsync(WorkoutCompletedEvent workoutEvent, CancellationToken cancellationToken)
    {
        var workout = await _goalRepository.GetSwimmingWorkoutByIdAsync(workoutEvent.WorkoutId, cancellationToken);
        if (workout == null) return;

        var goals = await _goalRepository.GetActiveGoalsByWorkoutTypeAsync(workoutEvent.UserId, "swimming", cancellationToken);

        foreach (var goal in goals)
        {
            var delta = CalculateDelta(goal, workout);
            if (delta <= 0) continue;

            await UpdateGoalWithDeltaAsync(goal, delta, workout.Id);
        }
    }

    private double CalculateDelta(UserGoal goal, backend.Modules.Workout.Domain.Entities.SwimmingUserWorkoutDetails workout)
    {
        var metric = goal.GoalType.RelatedMetric?.ToLowerInvariant();
        return metric switch
        {
            "distance" => (workout.DistanceMeters ?? 0) / 1000.0, // Normalize to km usually, or check Goal Unit?
            // Assuming Goal Unit matches Metric. If metric is 'distance', we assume KM for consistency with other types, 
            // BUT swimming is often measured in Meters.
            // GoalTypeSeeder used "m" for Swimming Distance ("Swim Total Distance").
            // So if Unit is 'm', we should return meters.
            // But 'DistanceMetricCalculator' (old) was extraction DistanceKm.
            // Let's check GoalType. 
            // If metric is "distance", let's return METERS if the Goal Unit is 'm' or 'meters', else KM.
            // Wait, we don't want complex unit conversion logic here without a library.
            // Seeder says: DefaultUnit="m" for Swim.
            // Let's assume the value should be in whatever the "Standard" for that sport is.
            // For Swim, it's Meters.
            // But 'GetTotalDistance()' on Workout returns Null or something? 
            // SwimmingUserWorkoutDetails.GetTotalDistance() returns KM (DistanceMeters / 1000.0).
            // Let's return Meters here as per Seeder expectation.
            // Actually, let's standardize on the GoalType's unit.
            _ when metric == "distance" => workout.DistanceMeters ?? 0,
            
            "laps" => workout.Laps ?? 0,
            "duration" => workout.DurationMinutes ?? 0,
            "count" => 1,
            _ => 0
        };
    }

    private async Task UpdateGoalWithDeltaAsync(UserGoal goal, double delta, Guid workoutId)
    {
        var previousValue = goal.CurrentValue;
        
        var timeZoneId = goal.User?.AppUserPreference?.TimeZoneId ?? "Central European Standard Time";
        TimeZoneInfo timeZone;
        try { timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId); }
        catch { timeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time"); }
        
        goal.UpdateProgress(delta, goal.GoalType.MeasurementType, DateTime.UtcNow, timeZone);

        var entry = new GoalProgressEntry(
            Guid.NewGuid(),
            goal.Id,
            previousValue,
            goal.CurrentValue,
            "workout",
            workoutId
        );

        await _goalRepository.AddProgressEntryAsync(entry);
        await _goalRepository.UpdateAsync(goal);
    }
}
