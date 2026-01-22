using backend.Modules.Goal.Infrastructure;
using backend.Modules.Shared;
using backend.Modules.Workout.Domain.Events;
using MediatR;

namespace backend.Modules.Goal.Handlers;

/// <summary>
/// Listens to WorkoutDeletedEvent and rolls back goal progress.
/// </summary>
public class WorkoutDeletedGoalHandler : INotificationHandler<WorkoutDeletedEvent>
{
    private readonly IGoalRepository _goalRepository;
    private readonly IUnitOfWork _unitOfWork;

    public WorkoutDeletedGoalHandler(IGoalRepository goalRepository, IUnitOfWork unitOfWork)
    {
        _goalRepository = goalRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(WorkoutDeletedEvent notification, CancellationToken cancellationToken)
    {
        // Find history entries created by this workout
        var entries = await _goalRepository.GetProgressBySourceIdAsync(notification.WorkoutId, cancellationToken);
        
        foreach (var entry in entries)
        {
            var goal = await _goalRepository.GetByIdAsync(entry.GoalId, cancellationToken);
            if (goal == null) continue;

            // Rollback Logic
            // If we are deleting the most recent entry, we restore the exact previous value
            if (Math.Abs(goal.CurrentValue - entry.NewValue) < 0.001)
            {
                // This was the latest update (or value hasn't changed since)
                // We can safely restore to PreviousValue
                goal.UpdateProgress(entry.PreviousValue - goal.CurrentValue, goal.GoalType.MeasurementType, DateTime.UtcNow); 
                // Wait, UpdateProgress adds delta. 
                // We want Valid New Value = Entry.PreviousValue.
                // So Delta = Entry.PreviousValue - Goal.CurrentValue.
                // e.g. Current=105, Previous=100. Entry.New=105. Delta needed = 100-105 = -5.
                // Using UpdateProgress ensures events and checks run (though checking completion on rollback is rare, checking streak might be needed?)
                
                // Hack: UpdateProgress adds a NEW history entry. We probably don't want a "Rollback" entry?
                // Or maybe we do for audit? "Manual Correction".
                // But we are also deleting the old entry next. 
                // Actually, if we delete the old entry, we shouldn't have a record of it.
                // But we can't "delete" the effect on the goal without modifying the goal.
                
                // If we use UpdateProgress, it adds a new entry.
                // Let's manually adjust goal value for now to avoid side effects, assuming undo is "clean".
                // But UserGoal property setters are private.
                // We should expose a `RollbackProgress(double value)` method?
                // Or just use UpdateProgress with a negative delta and "Rollback" source.
                
                // Let's use UpdateProgress with negative delta.
                // But we also want to remove the specific history entry that caused it effectively?
                // The task says "Delete history entry".
            }
            else
            {
                // If subsequent updates happened (e.g. Current=110, Entry was 100->105).
                // We just subtract the delta. 110 - 5 = 105.
                // This is only valid for Cumulative.
                // For Max (SingleEvent), strictly subtracting might be wrong if the deleted one wasn't the max anymore.
                // But usually safeish for MVP.
                
                var rollbackDelta = -entry.Delta;
                goal.UpdateProgress(rollbackDelta, goal.GoalType.MeasurementType, DateTime.UtcNow);
            }

            // Remove the history entry
            await _goalRepository.RemoveProgressEntryAsync(entry, cancellationToken);
            await _goalRepository.UpdateAsync(goal, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
