using backend.Modules.Goal.Domain.Entities;
using backend.Modules.Goal.Domain.Enums;
using backend.Modules.Goal.Infrastructure;
using backend.Modules.Shared;
using backend.Modules.Workout.Domain.Events;
using MediatR;

namespace backend.Modules.Goal.Handlers;

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
        var entries = (await _goalRepository.GetProgressBySourceIdAsync(notification.WorkoutId, cancellationToken))
            .Where(entry => entry.Source == "workout")
            .ToList();
        if (entries.Count == 0)
            return;

        foreach (var entriesByGoal in entries.GroupBy(entry => entry.GoalId))
        {
            var goal = await _goalRepository.GetByIdAsync(entriesByGoal.Key, cancellationToken);
            if (goal is null)
                continue;

            var removedEntryIds = entriesByGoal.Select(entry => entry.Id).ToHashSet();
            var history = await _goalRepository.GetProgressHistoryAsync(goal.Id, cancellationToken);
            var remainingHistory = history
                .Where(entry => !removedEntryIds.Contains(entry.Id))
                .ToList();
            var restoredValue = CalculateRestoredValue(goal, entriesByGoal, remainingHistory);

            goal.RestoreProgress(restoredValue);

            foreach (var entry in entriesByGoal)
            {
                await _goalRepository.RemoveProgressEntryAsync(entry, cancellationToken);
            }

            await _goalRepository.UpdateAsync(goal, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static double CalculateRestoredValue(
        UserGoal goal,
        IEnumerable<GoalProgressEntry> removedEntries,
        IReadOnlyCollection<GoalProgressEntry> remainingHistory)
    {
        return goal.GoalType.MeasurementType switch
        {
            GoalMeasurementType.Cumulative => goal.CurrentValue - removedEntries.Sum(entry => entry.Delta),
            GoalMeasurementType.SingleEvent => remainingHistory.Count == 0 ? 0 : remainingHistory.Max(entry => entry.NewValue),
            GoalMeasurementType.Threshold or GoalMeasurementType.Streak => remainingHistory
                .OrderByDescending(entry => entry.RecordedAt)
                .Select(entry => entry.NewValue)
                .FirstOrDefault(),
            _ => goal.CurrentValue
        };
    }
}
