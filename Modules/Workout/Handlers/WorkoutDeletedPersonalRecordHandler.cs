using backend.Modules.Shared;
using backend.Modules.Workout.Domain.Events;
using backend.Modules.Workout.Services;
using MediatR;

namespace backend.Modules.Workout.Handlers;

public class WorkoutDeletedPersonalRecordHandler : INotificationHandler<WorkoutDeletedEvent>
{
    private readonly IPersonalRecordRecalculationService _records; private readonly IUnitOfWork _unitOfWork;
    public WorkoutDeletedPersonalRecordHandler(IPersonalRecordRecalculationService records, IUnitOfWork unitOfWork) { _records = records; _unitOfWork = unitOfWork; }
    public async Task Handle(WorkoutDeletedEvent notification, CancellationToken cancellationToken) { await _records.RecalculateAsync(notification.UserId, cancellationToken); await _unitOfWork.SaveChangesAsync(cancellationToken); }
}
