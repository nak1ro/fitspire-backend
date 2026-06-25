using backend.Modules.Goal.Services;
using backend.Modules.Shared;
using backend.Modules.Workout.Domain.Events;
using MediatR;

namespace backend.Modules.Goal.Handlers;

public class WorkoutCompletedGoalHandler : INotificationHandler<WorkoutCompletedEvent>
{
    private readonly IGoalProgressService _progressService;
    private readonly IUnitOfWork _unitOfWork;

    public WorkoutCompletedGoalHandler(IGoalProgressService progressService, IUnitOfWork unitOfWork)
    {
        _progressService = progressService;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(WorkoutCompletedEvent notification, CancellationToken cancellationToken)
    {
        await _progressService.RecalculateForUserAsync(notification.UserId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
