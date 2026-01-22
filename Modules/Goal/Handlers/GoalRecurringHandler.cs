using backend.Modules.Goal.Domain.Entities;
using backend.Modules.Goal.Domain.Events;
using backend.Modules.Goal.Infrastructure;
using backend.Modules.Shared;
using MediatR;

namespace backend.Modules.Goal.Handlers;

public class GoalRecurringHandler : INotificationHandler<GoalRecurringEvent>
{
    private readonly IGoalRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public GoalRecurringHandler(IGoalRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(GoalRecurringEvent notification, CancellationToken cancellationToken)
    {
        // Calculate new dates
        var (newStart, newEnd) = CalculateNextPeriod(notification.OldDeadline, notification.RecurrencePattern);

        var newGoal = new UserGoal(
            Guid.NewGuid(),
            notification.UserId,
            notification.GoalTypeId,
            notification.TargetValue,
            notification.Unit,
            newStart,
            newEnd,
            true,
            notification.RecurrencePattern,
            false // Inherit privacy? Default false for now
        );

        await _repository.AddAsync(newGoal, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private (DateTime Start, DateTime End) CalculateNextPeriod(DateTime oldDeadline, string pattern)
    {
        var start = oldDeadline; // Next period starts when old one ends
        DateTime end;

        switch (pattern.ToLowerInvariant())
        {
            case "daily":
                end = start.AddDays(1);
                break;
            case "weekly":
                end = start.AddDays(7);
                break;
            case "monthly":
                end = start.AddMonths(1);
                break;
            default:
                end = start.AddDays(7); // Default fallback
                break;
        }

        return (start, end);
    }
}
