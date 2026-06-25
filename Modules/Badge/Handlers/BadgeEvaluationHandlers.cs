using backend.Modules.Badge.Services;
using backend.Modules.Shared;
using backend.Modules.Workout.Domain.Events;
using MediatR;

namespace backend.Modules.Badge.Handlers;

public class WorkoutBadgeEvaluationHandler : INotificationHandler<WorkoutCompletedEvent>
{
    private readonly IBadgeEvaluationService _badges; private readonly IUnitOfWork _unitOfWork;
    public WorkoutBadgeEvaluationHandler(IBadgeEvaluationService badges, IUnitOfWork unitOfWork) { _badges = badges; _unitOfWork = unitOfWork; }
    public async Task Handle(WorkoutCompletedEvent notification, CancellationToken cancellationToken) { await _badges.EvaluateAsync(notification.UserId, cancellationToken); await _unitOfWork.SaveChangesAsync(cancellationToken); }
}
