using backend.Data;
using backend.Modules.Badge.Services;
using backend.Modules.Notification.Domain.Constants;
using backend.Modules.Notification.Domain.Enums;
using backend.Modules.Notification.Services;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.Challenge.Services;

public interface IChallengeScoringService
{
    Task RecalculateForUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task ProcessLifecycleAsync(DateTime nowUtc, CancellationToken cancellationToken = default);
}

public class ChallengeScoringService : IChallengeScoringService
{
    private readonly FitspireDbContext _context;
    private readonly INotificationService _notifications;
    private readonly IBadgeEvaluationService _badges;
    public ChallengeScoringService(FitspireDbContext context, INotificationService notifications, IBadgeEvaluationService badges) { _context = context; _notifications = notifications; _badges = badges; }

    public async Task RecalculateForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var participants = await _context.ChallengeParticipants.Include(item => item.UserChallenge)
            .Where(item => item.UserId == userId && item.Status == "Active" && item.UserChallenge.Status == "Active")
            .ToListAsync(cancellationToken);
        foreach (var participant in participants)
        {
            var challenge = participant.UserChallenge;
            var score = await _context.ActivityContributions.Where(contribution => contribution.UserId == userId && contribution.IsActive &&
                contribution.MetricCode == challenge.MetricCode && contribution.OccurredAt >= challenge.StartDate && contribution.OccurredAt < challenge.EndDate &&
                contribution.OccurredAt >= participant.JoinedAt && (challenge.WorkoutType == null || contribution.WorkoutType == challenge.WorkoutType))
                .SumAsync(contribution => (double?)contribution.Value, cancellationToken) ?? 0;
            participant.Score = (float)score;
            var matching = await _context.ActivityContributions.Where(contribution => contribution.UserId == userId && contribution.IsActive &&
                contribution.MetricCode == challenge.MetricCode && contribution.OccurredAt >= challenge.StartDate && contribution.OccurredAt < challenge.EndDate && contribution.OccurredAt >= participant.JoinedAt &&
                (challenge.WorkoutType == null || contribution.WorkoutType == challenge.WorkoutType)).ToListAsync(cancellationToken);
            var existing = await _context.ChallengeScoreContributions.Where(item => item.ParticipantId == participant.Id).ToListAsync(cancellationToken);
            _context.ChallengeScoreContributions.RemoveRange(existing);
            await _context.ChallengeScoreContributions.AddRangeAsync(matching.Select(item => new Domain.ChallengeScoreContribution
            { Id = Guid.NewGuid(), ChallengeId = challenge.Id, ParticipantId = participant.Id, ActivityContributionId = item.Id, Value = item.Value }), cancellationToken);
        }
    }

    public async Task ProcessLifecycleAsync(DateTime nowUtc, CancellationToken cancellationToken = default)
    {
        var upcoming = await _context.Challenges.Where(item => item.Status == "Upcoming" && item.StartDate <= nowUtc).ToListAsync(cancellationToken);
        foreach (var challenge in upcoming)
        {
            challenge.Status = "Active";
            var users = await _context.ChallengeParticipants.Where(item => item.ChallengeId == challenge.Id && item.Status == "Active").Select(item => item.UserId).ToListAsync(cancellationToken);
            foreach (var userId in users)
                await _notifications.CreateAsync(userId, NotificationType.ChallengeStarted, $"{challenge.Title} has started.", referenceEntityId: challenge.Id, referenceEntityType: NotificationReferenceTypes.Challenge, cancellationToken: cancellationToken);
        }
        var ended = await _context.Challenges.Include(item => item.Participants).Where(item => item.Status == "Active" && item.EndDate <= nowUtc).ToListAsync(cancellationToken);
        foreach (var challenge in ended)
        {
            challenge.Status = "Finalizing";
            foreach (var participant in challenge.Participants.Where(item => item.Status == "Active")) await RecalculateForUserAsync(participant.UserId, cancellationToken);
            var ranking = challenge.Participants.Where(item => item.Status == "Active").OrderByDescending(item => item.Score).ToList();
            var rank = 0; var previousScore = float.NaN;
            for (var index = 0; index < ranking.Count; index++)
            {
                var participant = ranking[index]; if (participant.Score != previousScore) rank = index + 1; previousScore = participant.Score;
                if (!await _context.ChallengeResults.AnyAsync(result => result.ChallengeId == challenge.Id && result.ParticipantId == participant.Id, cancellationToken))
                {
                    await _context.ChallengeResults.AddAsync(new Domain.ChallengeResult { Id = Guid.NewGuid(), ChallengeId = challenge.Id, ParticipantId = participant.Id, UserId = participant.UserId,
                        Score = participant.Score, Rank = rank, IsFinisher = challenge.Mode == "Target" && participant.Score >= challenge.TargetValue, IsWinner = challenge.Mode == "Leaderboard" && rank == 1, FinalizedAt = nowUtc }, cancellationToken);
                    var success = challenge.Mode == "Target" ? participant.Score >= challenge.TargetValue : rank == 1;
                    if (success)
                        await _notifications.CreateAsync(participant.UserId, rank == 1 && challenge.Mode == "Leaderboard" ? NotificationType.ChallengeWon : NotificationType.ChallengeCompleted,
                            rank == 1 && challenge.Mode == "Leaderboard" ? $"You won {challenge.Title}." : $"You completed {challenge.Title}.", referenceEntityId: challenge.Id, referenceEntityType: NotificationReferenceTypes.Challenge, cancellationToken: cancellationToken);
                }
            }
            challenge.Status = "Completed"; challenge.CompletedAt = nowUtc;
            foreach (var participant in challenge.Participants.Where(item => item.Status == "Active"))
                await _badges.EvaluateAsync(participant.UserId, cancellationToken);
        }
    }
}
