using backend.Data;
using backend.Modules.Badge.Domain;
using backend.Modules.Notification.Domain.Constants;
using backend.Modules.Notification.Domain.Enums;
using backend.Modules.Notification.Services;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.Badge.Services;

public interface IBadgeEvaluationService { Task EvaluateAsync(Guid userId, CancellationToken cancellationToken = default); }

public class BadgeEvaluationService : IBadgeEvaluationService
{
    private readonly FitspireDbContext _context;
    private readonly INotificationService _notifications;
    public BadgeEvaluationService(FitspireDbContext context, INotificationService notifications) { _context = context; _notifications = notifications; }
    public async Task EvaluateAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var badges = await _context.Badges.Where(badge => badge.IsActive).ToListAsync(cancellationToken);
        foreach (var badge in badges)
        {
            var value = badge.CriterionCode switch
            {
                "WorkoutCount" => await _context.ActivityContributions.CountAsync(item => item.UserId == userId && item.IsActive && item.MetricCode == "workout.count", cancellationToken),
                "ChallengeFinishes" => await _context.ChallengeResults.CountAsync(item => item.UserId == userId && item.IsFinisher, cancellationToken),
                "ChallengeWins" => await _context.ChallengeResults.CountAsync(item => item.UserId == userId && item.IsWinner, cancellationToken),
                _ => 0
            };
            if (value >= badge.Threshold && !await _context.UserBadges.AnyAsync(item => item.UserId == userId && item.BadgeId == badge.Id, cancellationToken))
            {
                await _context.UserBadges.AddAsync(new UserBadge { Id = Guid.NewGuid(), UserId = userId, BadgeId = badge.Id, AchievedValue = value, EvidenceType = badge.CriterionCode, EvidenceSummary = $"Reached {value} for {badge.CriterionCode}." }, cancellationToken);
                await _notifications.CreateAsync(userId, NotificationType.BadgeEarned, $"You earned the {badge.Name} badge.", referenceEntityId: badge.Id, referenceEntityType: NotificationReferenceTypes.Badge, cancellationToken: cancellationToken);
            }
        }
    }
}
