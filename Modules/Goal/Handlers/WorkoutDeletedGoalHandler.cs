using backend.Modules.Goal.Services;
using backend.Modules.Shared;
using backend.Modules.Workout.Domain.Events;
using MediatR;

namespace backend.Modules.Goal.Handlers;

public class WorkoutDeletedGoalHandler : INotificationHandler<WorkoutDeletedEvent>
{
    private readonly IGoalProgressService _progressService;
    private readonly IUnitOfWork _unitOfWork;

    public WorkoutDeletedGoalHandler(IGoalProgressService progressService, IUnitOfWork unitOfWork)
    {
        _progressService = progressService;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(WorkoutDeletedEvent notification, CancellationToken cancellationToken)
    {
        await _progressService.RecalculateForUserAsync(notification.UserId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
