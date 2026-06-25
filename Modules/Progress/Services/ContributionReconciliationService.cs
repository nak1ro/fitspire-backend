using backend.Data;
using backend.Modules.Progress.Domain;
using backend.Modules.Workout.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.Progress.Services;

public interface IContributionReconciliationService
{
    Task ReconcileWorkoutAsync(UserWorkout workout, CancellationToken cancellationToken = default);
    Task DeactivateWorkoutAsync(Guid workoutId, CancellationToken cancellationToken = default);
}

public class ContributionReconciliationService : IContributionReconciliationService
{
    private readonly FitspireDbContext _context;

    public ContributionReconciliationService(FitspireDbContext context) => _context = context;

    public async Task ReconcileWorkoutAsync(UserWorkout workout, CancellationToken cancellationToken = default)
    {
        var existing = await _context.ActivityContributions
            .Where(contribution => contribution.SourceWorkoutId == workout.Id && contribution.IsActive)
            .ToListAsync(cancellationToken);
        foreach (var contribution in existing)
            contribution.Deactivate();

        foreach (var contribution in WorkoutContributionFactory.Create(workout))
            await _context.ActivityContributions.AddAsync(contribution, cancellationToken);
    }

    public async Task DeactivateWorkoutAsync(Guid workoutId, CancellationToken cancellationToken = default)
    {
        var contributions = await _context.ActivityContributions
            .Where(contribution => contribution.SourceWorkoutId == workoutId && contribution.IsActive)
            .ToListAsync(cancellationToken);
        foreach (var contribution in contributions)
            contribution.Deactivate();
    }
}
