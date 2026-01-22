using backend.Modules.Goal.Domain.Entities;
using backend.Modules.Goal.Infrastructure;
using backend.Modules.Shared;
using backend.Modules.Workout.Domain.Events;
using MediatR;

namespace backend.Modules.Goal.Handlers;

/// <summary>
/// Listens to WorkoutCompletedEvent from Workout module and updates relevant goals.
/// </summary>
public class WorkoutCompletedGoalHandler : INotificationHandler<WorkoutCompletedEvent>
{
    private readonly IGoalRepository _goalRepository;
    private readonly IUnitOfWork _unitOfWork;

    public WorkoutCompletedGoalHandler(IGoalRepository goalRepository, IUnitOfWork unitOfWork)
    {
        _goalRepository = goalRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(WorkoutCompletedEvent notification, CancellationToken cancellationToken)
    {
        // Find all active goals for this user that match the workout type
        var goals = await _goalRepository.GetActiveGoalsByWorkoutTypeAsync(
            notification.UserId, 
            notification.WorkoutType, 
            cancellationToken);

        foreach (var goal in goals)
        {
            var delta = CalculateDelta(goal, notification);
            if (delta == 0) continue;

            var previousValue = goal.CurrentValue;
            goal.UpdateProgress(delta, goal.GoalType.MeasurementType, DateTime.UtcNow);

            // Record progress history
            var entry = new GoalProgressEntry(
                Guid.NewGuid(),
                goal.Id,
                previousValue,
                goal.CurrentValue,
                "workout",
                notification.WorkoutId
            );

            await _goalRepository.AddProgressEntryAsync(entry, cancellationToken);
            await _goalRepository.UpdateAsync(goal, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private double CalculateDelta(UserGoal goal, WorkoutCompletedEvent workout)
    {
        var metric = goal.GoalType.RelatedMetric?.ToLowerInvariant();

        return metric switch
        {
            "count" => 1, // One workout completed
            "duration" => workout.DurationMinutes ?? 0,
            // For distance-based goals, the workout event would need to carry distance data
            // We'll default to count for now
            _ => 1
        };
    }
}
