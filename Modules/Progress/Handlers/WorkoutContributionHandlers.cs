using backend.Data;
using backend.Modules.Progress.Services;
using backend.Modules.Shared;
using backend.Modules.Workout.Domain.Entities;
using backend.Modules.Workout.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.Progress.Handlers;

public class WorkoutCompletedContributionHandler : INotificationHandler<WorkoutCompletedEvent>
{
    private readonly FitspireDbContext _context;
    private readonly IContributionReconciliationService _reconciliation;
    private readonly IUnitOfWork _unitOfWork;

    public WorkoutCompletedContributionHandler(FitspireDbContext context, IContributionReconciliationService reconciliation, IUnitOfWork unitOfWork)
    {
        _context = context;
        _reconciliation = reconciliation;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(WorkoutCompletedEvent notification, CancellationToken cancellationToken)
    {
        var workout = await _context.UserWorkouts
            .Include(item => ((GymUserWorkoutDetails)item).Exercises)
            .FirstOrDefaultAsync(item => item.Id == notification.WorkoutId, cancellationToken);
        if (workout is not null)
        {
            await _reconciliation.ReconcileWorkoutAsync(workout, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}

public class WorkoutDeletedContributionHandler : INotificationHandler<WorkoutDeletedEvent>
{
    private readonly IContributionReconciliationService _reconciliation;
    private readonly IUnitOfWork _unitOfWork;

    public WorkoutDeletedContributionHandler(IContributionReconciliationService reconciliation, IUnitOfWork unitOfWork)
    {
        _reconciliation = reconciliation;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(WorkoutDeletedEvent notification, CancellationToken cancellationToken)
    {
        await _reconciliation.DeactivateWorkoutAsync(notification.WorkoutId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
