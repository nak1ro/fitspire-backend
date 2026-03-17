using backend.Modules.Goal.Domain.Entities;
using backend.Modules.Goal.Infrastructure;
using backend.Modules.Goal.Services.MetricCalculators;
using backend.Modules.Shared;
using backend.Modules.Workout.Domain.Entities;
using backend.Modules.Workout.Domain.Events;

namespace backend.Modules.Goal.Services.GoalProcessors;

public class GymGoalProcessor : IWorkoutGoalProcessor
{
    private readonly IGoalRepository _goalRepository;
    private readonly IEnumerable<IExerciseMetricCalculator> _exerciseCalculators;

    public string SupportedWorkoutType => "gym";

    public GymGoalProcessor(
        IGoalRepository goalRepository, 
        IEnumerable<IExerciseMetricCalculator> exerciseCalculators)
    {
        _goalRepository = goalRepository;
        _exerciseCalculators = exerciseCalculators;
    }

    public async Task ProcessAsync(WorkoutCompletedEvent workoutEvent, CancellationToken cancellationToken)
    {
        var gymWorkout = await _goalRepository.GetGymWorkoutByIdAsync(workoutEvent.WorkoutId, cancellationToken);
        if (gymWorkout == null) return;

        var workoutGoals = await _goalRepository.GetActiveGoalsByWorkoutTypeAsync(workoutEvent.UserId, "gym", cancellationToken);
        foreach (var goal in workoutGoals.Where(g => g.GoalType.RelatedExerciseId == null))
        {
            var delta = CalculateWorkoutDelta(goal, gymWorkout);
            if (delta <= 0) continue;

            await UpdateGoalWithDeltaAsync(goal, delta, workoutEvent.WorkoutId);
        }

        foreach (var exercise in gymWorkout.Exercises)
        {
            var exerciseGoals = await _goalRepository.GetActiveGoalsByExerciseIdAsync(workoutEvent.UserId, exercise.ExerciseId, cancellationToken);
            foreach (var goal in exerciseGoals)
            {
                await UpdateExerciseGoalProgressAsync(goal, exercise, workoutEvent.WorkoutId);
            }
        }
    }

    private static double CalculateWorkoutDelta(UserGoal goal, GymUserWorkoutDetails workout)
    {
        var metric = goal.GoalType.RelatedMetric?.ToLowerInvariant();
        return metric switch
        {
            "count" => 1,
            "volume" => workout.CalculateTotalVolume(),
            "weight" => workout.GetMaxWeight() ?? 0,
            _ => 0
        };
    }

    private async Task UpdateExerciseGoalProgressAsync(UserGoal goal, GymWorkoutExercise exercise, Guid workoutId)
    {
        var delta = CalculateExerciseDelta(goal, exercise);
        if (delta <= 0) return;
        
        await UpdateGoalWithDeltaAsync(goal, delta, workoutId);
    }
    
    private double CalculateExerciseDelta(UserGoal goal, GymWorkoutExercise exercise)
    {
        var metricName = goal.GoalType.RelatedMetric?.ToLowerInvariant() ?? "count";
        var calculator = _exerciseCalculators.FirstOrDefault(c => c.MetricName.Equals(metricName, StringComparison.InvariantCultureIgnoreCase));
        return calculator?.Calculate(exercise) ?? 0;
    }

    private async Task UpdateGoalWithDeltaAsync(UserGoal goal, double delta, Guid workoutId)
    {
        var previousValue = goal.CurrentValue;
        
        // Get timezone
        var timeZoneId = goal.User?.AppUserPreference?.TimeZoneId ?? "Central European Standard Time";
        TimeZoneInfo timeZone;
        try 
        {
            timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch
        {
            timeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
        }
        
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
