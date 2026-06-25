using backend.Data;
using backend.Modules.Badge.Services;
using backend.Modules.Challenge.Domain;
using backend.Modules.Challenge.Domain.Constants;
using backend.Modules.Notification.Domain.Constants;
using backend.Modules.Notification.Domain.Enums;
using backend.Modules.Notification.Services;
using backend.Modules.Shared;
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
    private readonly IChallengeMetricService _metrics;
    private readonly IChallengeTransactionService _transactions;
    private readonly INotificationService _notifications;
    private readonly IBadgeEvaluationService _badges;
    private readonly IUnitOfWork _unitOfWork;

    public ChallengeScoringService(FitspireDbContext context, IChallengeMetricService metrics, IChallengeTransactionService transactions,
        INotificationService notifications, IBadgeEvaluationService badges, IUnitOfWork unitOfWork)
    {
        _context = context;
        _metrics = metrics;
        _transactions = transactions;
        _notifications = notifications;
        _badges = badges;
        _unitOfWork = unitOfWork;
    }

    public async Task RecalculateForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var participants = await _context.ChallengeParticipants.Include(item => item.UserChallenge)
            .Where(item => item.UserId == userId && item.Status == ChallengeParticipantStatuses.Active &&
                (item.UserChallenge.Status == ChallengeStatuses.Active || item.UserChallenge.Status == ChallengeStatuses.Finalizing))
            .ToListAsync(cancellationToken);

        foreach (var participant in participants)
            await RecalculateParticipantAsync(participant, cancellationToken);
    }

    public Task ProcessLifecycleAsync(DateTime nowUtc, CancellationToken cancellationToken = default) =>
        _transactions.ExecuteAsync(token => ProcessLifecycleCoreAsync(nowUtc, token), cancellationToken);

    private async Task ProcessLifecycleCoreAsync(DateTime nowUtc, CancellationToken cancellationToken)
    {
        var upcoming = await _context.Challenges.Include(item => item.Participants).Include(item => item.Invitations)
            .Where(item => item.Status == ChallengeStatuses.Upcoming && item.StartDate <= nowUtc)
            .ToListAsync(cancellationToken);
        foreach (var challenge in upcoming)
            await StartChallengeAsync(challenge, nowUtc, cancellationToken);

        var ended = await _context.Challenges.Include(item => item.Participants).Include(item => item.Invitations)
            .Where(item => item.Status == ChallengeStatuses.Active && item.EndDate <= nowUtc)
            .ToListAsync(cancellationToken);
        foreach (var challenge in ended)
            await FinalizeChallengeAsync(challenge, nowUtc, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task StartChallengeAsync(UserChallenge challenge, DateTime nowUtc, CancellationToken cancellationToken)
    {
        challenge.Start(nowUtc);
        foreach (var invitation in challenge.Invitations.Where(item => item.Status == ChallengeInvitationStatuses.Pending &&
                     challenge.JoinClosing == ChallengeJoinClosingModes.AtStart))
            invitation.Expire(nowUtc);

        foreach (var participant in challenge.Participants.Where(item => item.Status == ChallengeParticipantStatuses.Active))
        {
            await _notifications.CreateAsync(participant.UserId, NotificationType.ChallengeStarted, $"{challenge.Title} has started.",
                referenceEntityId: challenge.Id, referenceEntityType: NotificationReferenceTypes.Challenge, cancellationToken: cancellationToken);
            await RecalculateParticipantAsync(participant, cancellationToken);
        }
    }

    private async Task FinalizeChallengeAsync(UserChallenge challenge, DateTime nowUtc, CancellationToken cancellationToken)
    {
        challenge.BeginFinalization(nowUtc);
        foreach (var invitation in challenge.Invitations.Where(item => item.Status == ChallengeInvitationStatuses.Pending))
            invitation.Expire(nowUtc);
        foreach (var participant in challenge.Participants.Where(item => item.Status == ChallengeParticipantStatuses.Active))
            await RecalculateParticipantAsync(participant, cancellationToken);

        var ranking = challenge.Participants.Where(item => item.Status == ChallengeParticipantStatuses.Active)
            .OrderByDescending(item => item.Score).ToList();
        var rank = 0;
        var previousScore = float.NaN;
        for (var index = 0; index < ranking.Count; index++)
        {
            var participant = ranking[index];
            if (participant.Score != previousScore) rank = index + 1;
            previousScore = participant.Score;
            await CreateResultAsync(challenge, participant, rank, nowUtc, cancellationToken);
        }

        challenge.Complete(nowUtc);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        foreach (var participant in ranking)
            await _badges.EvaluateAsync(participant.UserId, cancellationToken);
    }

    private async Task RecalculateParticipantAsync(ChallengeParticipant participant, CancellationToken cancellationToken)
    {
        var challenge = participant.UserChallenge;
        var contributions = await _context.ActivityContributions.Where(item => item.UserId == participant.UserId && item.IsActive &&
                item.MetricCode == challenge.MetricCode && item.OccurredAt >= challenge.StartDate && item.OccurredAt < challenge.EndDate &&
                item.OccurredAt >= participant.JoinedAt && (challenge.WorkoutType == null || item.WorkoutType == challenge.WorkoutType))
            .ToListAsync(cancellationToken);
        participant.SetScore(await AggregateScoreAsync(challenge.MetricCode, contributions.Select(item => item.Value), cancellationToken));

        var existing = await _context.ChallengeScoreContributions.Where(item => item.ParticipantId == participant.Id).ToListAsync(cancellationToken);
        foreach (var contribution in contributions)
        {
            var scoreContribution = existing.SingleOrDefault(item => item.ActivityContributionId == contribution.Id);
            if (scoreContribution is null)
                await _context.ChallengeScoreContributions.AddAsync(new ChallengeScoreContribution
                {
                    Id = Guid.NewGuid(), ChallengeId = challenge.Id, ParticipantId = participant.Id,
                    ActivityContributionId = contribution.Id, Value = contribution.Value
                }, cancellationToken);
            else
                scoreContribution.Value = contribution.Value;
        }

        _context.ChallengeScoreContributions.RemoveRange(existing.Where(item => contributions.All(contribution => contribution.Id != item.ActivityContributionId)));
    }

    private async Task<double> AggregateScoreAsync(string metricCode, IEnumerable<double> values, CancellationToken cancellationToken)
    {
        var list = values.ToList();
        var aggregation = await _metrics.GetAggregationAsync(metricCode, cancellationToken);
        return aggregation == "Maximum" ? list.DefaultIfEmpty(0).Max() : list.Sum();
    }

    private async Task CreateResultAsync(UserChallenge challenge, ChallengeParticipant participant, int rank, DateTime nowUtc, CancellationToken cancellationToken)
    {
        var exists = await _context.ChallengeResults.AnyAsync(item => item.ChallengeId == challenge.Id && item.ParticipantId == participant.Id, cancellationToken);
        if (exists) return;

        var isFinisher = challenge.Mode == ChallengeModes.Target && participant.Score >= challenge.TargetValue;
        var isWinner = challenge.Mode == ChallengeModes.Leaderboard && rank == 1;
        await _context.ChallengeResults.AddAsync(new ChallengeResult
        {
            Id = Guid.NewGuid(), ChallengeId = challenge.Id, ParticipantId = participant.Id, UserId = participant.UserId,
            Score = participant.Score, Rank = rank, IsFinisher = isFinisher, IsWinner = isWinner, FinalizedAt = nowUtc
        }, cancellationToken);

        if (!isFinisher && !isWinner) return;
        var type = isWinner ? NotificationType.ChallengeWon : NotificationType.ChallengeCompleted;
        var message = isWinner ? $"You won {challenge.Title}." : $"You completed {challenge.Title}.";
        await _notifications.CreateAsync(participant.UserId, type, message, referenceEntityId: challenge.Id,
            referenceEntityType: NotificationReferenceTypes.Challenge, cancellationToken: cancellationToken);
    }
}
