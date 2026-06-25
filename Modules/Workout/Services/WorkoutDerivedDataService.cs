using backend.Data;
using backend.Modules.Badge.Services;
using backend.Modules.Challenge.Services;
using backend.Modules.Goal.Services;
using backend.Modules.Progress.Services;
using backend.Modules.Shared;
using backend.Modules.Workout.Domain.Entities;

namespace backend.Modules.Workout.Services;

public interface IWorkoutDerivedDataService
{
    Task ReconcileCompletedWorkoutAsync(UserWorkout workout, CancellationToken cancellationToken = default);
    Task ReconcileDeletedWorkoutAsync(Guid userId, Guid workoutId, CancellationToken cancellationToken = default);
}

public class WorkoutDerivedDataService : IWorkoutDerivedDataService
{
    private readonly FitspireDbContext _context;
    private readonly IContributionReconciliationService _contributions;
    private readonly IGoalProgressService _goals;
    private readonly IChallengeScoringService _challenges;
    private readonly IPersonalRecordRecalculationService _records;
    private readonly IBadgeEvaluationService _badges;
    private readonly IUnitOfWork _unitOfWork;

    public WorkoutDerivedDataService(FitspireDbContext context, IContributionReconciliationService contributions, IGoalProgressService goals,
        IChallengeScoringService challenges, IPersonalRecordRecalculationService records, IBadgeEvaluationService badges, IUnitOfWork unitOfWork)
    {
        _context = context; _contributions = contributions; _goals = goals; _challenges = challenges;
        _records = records; _badges = badges; _unitOfWork = unitOfWork;
    }

    public async Task ReconcileCompletedWorkoutAsync(UserWorkout workout, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        await _contributions.ReconcileWorkoutAsync(workout, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        var completedGoalPeriods = await RecalculateConsumersAsync(workout.UserId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        var triggers = new List<BadgeTriggerContext> { BadgeTriggerContext.ForWorkout(workout.Id) };
        triggers.AddRange(completedGoalPeriods.Select(BadgeTriggerContext.ForGoalPeriod));
        await _badges.EvaluateAsync(workout.UserId, triggers, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task ReconcileDeletedWorkoutAsync(Guid userId, Guid workoutId, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        await _contributions.DeactivateWorkoutAsync(workoutId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _goals.RecalculateForUserAsync(userId, cancellationToken);
        await _challenges.RecalculateForUserAsync(userId, cancellationToken);
        await _records.RecalculateAsync(userId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private async Task<IReadOnlyList<Guid>> RecalculateConsumersAsync(Guid userId, CancellationToken cancellationToken)
    {
        var completedGoalPeriods = await _goals.RecalculateForUserAsync(userId, cancellationToken);
        await _challenges.RecalculateForUserAsync(userId, cancellationToken);
        await _records.RecalculateAsync(userId, cancellationToken);
        return completedGoalPeriods;
    }
}
