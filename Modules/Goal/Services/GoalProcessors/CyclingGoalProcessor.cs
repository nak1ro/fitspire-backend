using backend.Modules.Goal.Domain.Entities;
using backend.Modules.Goal.Infrastructure;
using backend.Modules.Workout.Domain.Events;

namespace backend.Modules.Goal.Services.GoalProcessors;

public class CyclingGoalProcessor : IWorkoutGoalProcessor
{
    private readonly IGoalRepository _goalRepository;

    public string SupportedWorkoutType => "cycling";

    public CyclingGoalProcessor(IGoalRepository goalRepository)
    {
        _goalRepository = goalRepository;
    }

    public async Task ProcessAsync(WorkoutCompletedEvent workoutEvent, CancellationToken cancellationToken)
    {
        var workout = await _goalRepository.GetCyclingWorkoutByIdAsync(workoutEvent.WorkoutId, cancellationToken);
        if (workout == null) return;

        var goals = await _goalRepository.GetActiveGoalsByWorkoutTypeAsync(workoutEvent.UserId, "cycling", cancellationToken);

        foreach (var goal in goals)
        {
            var delta = CalculateDelta(goal, workout);
            if (delta <= 0) continue;

            await UpdateGoalWithDeltaAsync(goal, delta, workout.Id);
        }
    }

    private double CalculateDelta(UserGoal goal, backend.Modules.Workout.Domain.Entities.CyclingUserWorkoutDetails workout)
    {
        var metric = goal.GoalType.RelatedMetric?.ToLowerInvariant();
        return metric switch
        {
            "distance" => workout.DistanceKm,
            "elevation" => workout.ElevationGainMeters ?? 0,
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
