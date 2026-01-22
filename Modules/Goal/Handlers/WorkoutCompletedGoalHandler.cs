using backend.Modules.Goal.Domain.Entities;
using backend.Modules.Goal.Infrastructure;
using backend.Modules.Goal.Services.MetricCalculators;
using backend.Modules.Shared;
using backend.Modules.Workout.Domain.Entities;
using backend.Modules.Workout.Domain.Events;
using backend.Modules.Goal.Services.GoalProcessors;
using MediatR;

namespace backend.Modules.Goal.Handlers;

/// <summary>
/// Listens to WorkoutCompletedEvent from Workout module and updates relevant goals.
/// </summary>
public class WorkoutCompletedGoalHandler : INotificationHandler<WorkoutCompletedEvent>
{
    private readonly IEnumerable<IWorkoutGoalProcessor> _processors;
    private readonly IGoalRepository _goalRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEnumerable<IMetricCalculator> _metricCalculators;

    public WorkoutCompletedGoalHandler(
        IEnumerable<IWorkoutGoalProcessor> processors,
        IGoalRepository goalRepository, 
        IUnitOfWork unitOfWork,
        IEnumerable<IMetricCalculator> metricCalculators)
    {
        _processors = processors;
        _goalRepository = goalRepository;
        _unitOfWork = unitOfWork;
        _metricCalculators = metricCalculators;
    }

    public async Task Handle(WorkoutCompletedEvent notification, CancellationToken cancellationToken)
    {
        // 1. Find Processor for this Workout Type
        var processor = _processors.FirstOrDefault(p => p.SupportedWorkoutType.Equals(notification.WorkoutType, StringComparison.InvariantCultureIgnoreCase));
        
        if (processor != null)
        {
            await processor.ProcessAsync(notification, cancellationToken);
        }
        
        // 2. Handle "Generic/Cross-Type" Goals (e.g. "Burn Calories", "Any Workout Count")
        // These are goals where RelatedWorkoutType is "any" or null
        var genericGoals = await _goalRepository.GetActiveGoalsByWorkoutTypeAsync(
            notification.UserId, 
            "any", 
            cancellationToken);

        foreach (var goal in genericGoals)
        {
             await UpdateGoalProgressAsync(goal, notification, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
    
    private async Task UpdateGoalProgressAsync(UserGoal goal, WorkoutCompletedEvent workout, CancellationToken cancellationToken)
    {
        if (goal.GoalType.RelatedMetric == "calories" && workout.CaloriesBurned.HasValue)
        {
             await UpdateGoalWithDeltaAsync(goal, workout.CaloriesBurned.Value, workout.WorkoutId, cancellationToken);
        }
        else if (goal.GoalType.RelatedMetric == "count" || goal.GoalType.RelatedMetric == null)
        {
             await UpdateGoalWithDeltaAsync(goal, 1, workout.WorkoutId, cancellationToken);
        }
    }
    
    private async Task UpdateGoalWithDeltaAsync(UserGoal goal, double delta, Guid workoutId, CancellationToken cancellationToken)
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

        await _goalRepository.AddProgressEntryAsync(entry, cancellationToken);
        await _goalRepository.UpdateAsync(goal, cancellationToken);
    }
}
