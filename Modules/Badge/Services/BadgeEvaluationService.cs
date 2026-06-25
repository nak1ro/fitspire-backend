using backend.Data;
using backend.Modules.Badge.Domain;
using backend.Modules.Badge.Domain.Constants;
using backend.Modules.Notification.Domain.Constants;
using backend.Modules.Notification.Domain.Enums;
using backend.Modules.Notification.Services;
using backend.Modules.Shared;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.Badge.Services;

public interface IBadgeEvaluationService
{
    Task EvaluateAsync(Guid userId, CancellationToken cancellationToken = default);
    Task EvaluateAsync(Guid userId, IReadOnlyCollection<BadgeTriggerContext> triggers, CancellationToken cancellationToken = default);
}

public class BadgeEvaluationService : IBadgeEvaluationService
{
    private readonly FitspireDbContext _context;
    private readonly IBadgeAchievementSnapshotService _snapshots;
    private readonly IBadgeTransactionService _transactions;
    private readonly IBadgeUserLockService _locks;
    private readonly INotificationService _notifications;
    private readonly IUnitOfWork _unitOfWork;

    public BadgeEvaluationService(FitspireDbContext context, IBadgeAchievementSnapshotService snapshots,
        IBadgeTransactionService transactions, IBadgeUserLockService locks, INotificationService notifications, IUnitOfWork unitOfWork)
    {
        _context = context;
        _snapshots = snapshots;
        _transactions = transactions;
        _locks = locks;
        _notifications = notifications;
        _unitOfWork = unitOfWork;
    }

    public Task EvaluateAsync(Guid userId, CancellationToken cancellationToken = default) =>
        EvaluateAsync(userId, Array.Empty<BadgeTriggerContext>(), cancellationToken);

    public async Task EvaluateAsync(Guid userId, IReadOnlyCollection<BadgeTriggerContext> triggers,
        CancellationToken cancellationToken = default)
    {
        var ownsTransaction = _context.Database.CurrentTransaction is null;
        await _transactions.ExecuteAsync(async token =>
        {
            await _locks.AcquireAsync(userId, token);
            await EvaluateCoreAsync(userId, triggers, token);
            if (ownsTransaction)
                await _unitOfWork.SaveChangesAsync(token);
        }, cancellationToken);
    }

    private async Task EvaluateCoreAsync(Guid userId, IReadOnlyCollection<BadgeTriggerContext> triggers,
        CancellationToken cancellationToken)
    {
        var definitions = await _context.Badges.Where(badge => badge.IsActive).OrderBy(badge => badge.DisplayOrder)
            .ToListAsync(cancellationToken);
        var awardedBadgeIds = await _context.UserBadges.Where(award => award.UserId == userId)
            .Select(award => award.BadgeId).ToHashSetAsync(cancellationToken);
        awardedBadgeIds.UnionWith(_context.ChangeTracker.Entries<UserBadge>()
            .Where(entry => entry.State == EntityState.Added && entry.Entity.UserId == userId)
            .Select(entry => entry.Entity.BadgeId));
        var snapshot = await _snapshots.CreateAsync(userId, cancellationToken);

        foreach (var badge in definitions)
        {
            if (awardedBadgeIds.Contains(badge.Id))
                continue;

            var achievedValue = snapshot.GetValue(badge.CriterionCode);
            if (achievedValue < badge.Threshold)
                continue;

            var evidence = CreateEvidence(badge, achievedValue, triggers);
            await _context.UserBadges.AddAsync(UserBadge.Award(userId, badge, evidence), cancellationToken);
            await _notifications.CreateAsync(userId, NotificationType.BadgeEarned, $"You earned the {badge.Name} badge.",
                referenceEntityId: badge.Id, referenceEntityType: NotificationReferenceTypes.Badge, cancellationToken: cancellationToken);
            awardedBadgeIds.Add(badge.Id);
        }
    }

    private static BadgeAwardEvidence CreateEvidence(AchievementBadge badge, double achievedValue,
        IReadOnlyCollection<BadgeTriggerContext> triggers)
    {
        var trigger = FindApplicableTrigger(badge.CriterionCode, triggers);
        return new BadgeAwardEvidence(badge.CriterionCode, badge.Threshold, achievedValue, badge.CanonicalUnit,
            trigger?.EntityType, trigger?.EntityId, $"Reached {achievedValue} {badge.CanonicalUnit} for {badge.Name}.");
    }

    private static BadgeTriggerContext? FindApplicableTrigger(string criterionCode, IReadOnlyCollection<BadgeTriggerContext> triggers)
    {
        var triggerType = criterionCode switch
        {
            BadgeCriterionCodes.WorkoutCount or BadgeCriterionCodes.WorkoutLongestStreakDays or
                BadgeCriterionCodes.RunningDistanceTotalKm or BadgeCriterionCodes.CyclingDistanceTotalKm or
                BadgeCriterionCodes.SwimmingDistanceTotalMeters or BadgeCriterionCodes.WorkoutDurationTotalMinutes or
                BadgeCriterionCodes.GymVolumeTotalKg or BadgeCriterionCodes.PersonalRecordAchievementCount => BadgeTriggerTypes.Workout,
            BadgeCriterionCodes.GoalPeriodCompletionCount => BadgeTriggerTypes.GoalPeriod,
            BadgeCriterionCodes.ChallengeParticipationCount or BadgeCriterionCodes.ChallengeTargetCompletionCount or
                BadgeCriterionCodes.ChallengeWinCount => BadgeTriggerTypes.ChallengeResult,
            BadgeCriterionCodes.WorkoutShareCount => BadgeTriggerTypes.SocialPost,
            _ => throw new InvalidOperationException($"Badge criterion '{criterionCode}' has no trigger mapping.")
        };
        return triggers.FirstOrDefault(trigger => trigger.EntityType == triggerType);
    }
}
