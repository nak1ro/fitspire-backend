using backend.Modules.Challenge.Services;
using backend.Modules.Shared;
using backend.Modules.Workout.Domain.Events;
using MediatR;

namespace backend.Modules.Challenge.Handlers;

public class WorkoutCompletedChallengeHandler : INotificationHandler<WorkoutCompletedEvent>, INotificationHandler<WorkoutDeletedEvent>
{
    private readonly IChallengeScoringService _scoring; private readonly IUnitOfWork _unitOfWork;
    public WorkoutCompletedChallengeHandler(IChallengeScoringService scoring, IUnitOfWork unitOfWork) { _scoring = scoring; _unitOfWork = unitOfWork; }
    public async Task Handle(WorkoutCompletedEvent notification, CancellationToken cancellationToken) { await _scoring.RecalculateForUserAsync(notification.UserId, cancellationToken); await _unitOfWork.SaveChangesAsync(cancellationToken); }
    public async Task Handle(WorkoutDeletedEvent notification, CancellationToken cancellationToken) { await _scoring.RecalculateForUserAsync(notification.UserId, cancellationToken); await _unitOfWork.SaveChangesAsync(cancellationToken); }
}
