using backend.Data;
using backend.Modules.Workout.Domain.Entities;
using backend.Modules.Workout.Domain.Events;
using backend.Modules.Workout.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.Workout.Handlers;

public class WorkoutCompletedDerivedDataHandler : INotificationHandler<WorkoutCompletedEvent>
{
    private readonly FitspireDbContext _context;
    private readonly IWorkoutDerivedDataService _derivedData;

    public WorkoutCompletedDerivedDataHandler(FitspireDbContext context, IWorkoutDerivedDataService derivedData)
    {
        _context = context;
        _derivedData = derivedData;
    }

    public async Task Handle(WorkoutCompletedEvent notification, CancellationToken cancellationToken)
    {
        var workout = await _context.UserWorkouts.Include(item => ((GymUserWorkoutDetails)item).Exercises)
            .FirstOrDefaultAsync(item => item.Id == notification.WorkoutId, cancellationToken);
        if (workout is not null)
            await _derivedData.ReconcileCompletedWorkoutAsync(workout, cancellationToken);
    }
}

public class WorkoutDeletedDerivedDataHandler : INotificationHandler<WorkoutDeletedEvent>
{
    private readonly IWorkoutDerivedDataService _derivedData;
    public WorkoutDeletedDerivedDataHandler(IWorkoutDerivedDataService derivedData) => _derivedData = derivedData;
    public Task Handle(WorkoutDeletedEvent notification, CancellationToken cancellationToken) =>
        _derivedData.ReconcileDeletedWorkoutAsync(notification.UserId, notification.WorkoutId, cancellationToken);
}
