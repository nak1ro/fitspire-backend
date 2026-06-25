using backend.Data;
using backend.Modules.Challenge.Contracts;
using backend.Modules.Challenge.Domain;
using backend.Modules.Challenge.Domain.Constants;
using backend.Modules.Challenge.Services;
using backend.Modules.Notification.Domain.Constants;
using backend.Modules.Notification.Domain.Enums;
using backend.Modules.Notification.Services;
using backend.Modules.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.Challenge.Features;

public record CreateChallengeCommand(Guid UserId, CreateChallengeRequest Request) : IRequest<Guid>;
public record UpdateChallengeCommand(Guid UserId, Guid ChallengeId, UpdateChallengeRequest Request) : IRequest;
public record CancelChallengeCommand(Guid UserId, Guid ChallengeId) : IRequest;
public record RemoveChallengeParticipantCommand(Guid UserId, Guid ChallengeId, Guid ParticipantUserId) : IRequest;

public class CreateChallengeHandler : IRequestHandler<CreateChallengeCommand, Guid>
{
    private readonly FitspireDbContext _context;
    private readonly IChallengeMetricService _metrics;
    private readonly IChallengeTransactionService _transactions;

    public CreateChallengeHandler(FitspireDbContext context, IChallengeMetricService metrics, IChallengeTransactionService transactions)
    {
        _context = context;
        _metrics = metrics;
        _transactions = transactions;
    }

    public Task<Guid> Handle(CreateChallengeCommand request, CancellationToken cancellationToken) =>
        _transactions.ExecuteAsync(async token =>
        {
            await _metrics.EnsureSupportedAsync(request.Request.MetricCode, request.Request.WorkoutType, token);
            var ownedCount = await _context.Challenges.CountAsync(item => item.CreatedBy == request.UserId &&
                (item.Status == ChallengeStatuses.Upcoming || item.Status == ChallengeStatuses.Active), token);
            if (ownedCount >= 5) throw new DomainException("You can own at most five upcoming or active challenges.");

            var nowUtc = DateTime.UtcNow;
            var challenge = UserChallenge.Create(request.UserId, request.Request.Title, request.Request.Description,
                request.Request.MetricCode, request.Request.WorkoutType, request.Request.Mode, request.Request.TargetValue,
                request.Request.Visibility, request.Request.StartDate.ToUniversalTime(), request.Request.EndDate.ToUniversalTime(),
                request.Request.JoinClosing, request.Request.ParticipantLimit, nowUtc);

            await _context.Challenges.AddAsync(challenge, token);
            await _context.ChallengeParticipants.AddAsync(ChallengeParticipant.Create(challenge.Id, request.UserId, nowUtc), token);
            await _context.SaveChangesAsync(token);
            return challenge.Id;
        }, cancellationToken);
}

public class UpdateChallengeHandler : IRequestHandler<UpdateChallengeCommand>
{
    private readonly FitspireDbContext _context;
    private readonly IChallengeMetricService _metrics;
    private readonly IChallengeTransactionService _transactions;
    private readonly INotificationService _notifications;

    public UpdateChallengeHandler(FitspireDbContext context, IChallengeMetricService metrics,
        IChallengeTransactionService transactions, INotificationService notifications)
    {
        _context = context;
        _metrics = metrics;
        _transactions = transactions;
        _notifications = notifications;
    }

    public Task Handle(UpdateChallengeCommand request, CancellationToken cancellationToken) =>
        _transactions.ExecuteAsync(async token =>
        {
            var challenge = await _context.Challenges.Include(item => item.Participants).Include(item => item.Invitations)
                .FirstOrDefaultAsync(item => item.Id == request.ChallengeId, token) ?? throw new NotFoundException("Challenge not found.");
            if (challenge.CreatedBy != request.UserId) throw new UnauthorizedAccessException("Only the creator can edit a challenge.");

            await _metrics.EnsureSupportedAsync(request.Request.MetricCode, request.Request.WorkoutType, token);
            var activeCount = challenge.Participants.Count(item => item.Status == ChallengeParticipantStatuses.Active);
            var wasInviteOnly = challenge.Visibility == ChallengeVisibilities.InviteOnly;
            var pendingInviteeIds = challenge.Invitations.Where(item => item.Status == ChallengeInvitationStatuses.Pending)
                .Select(item => item.InvitedUserId).ToList();
            challenge.UpdateBeforeStart(request.Request.Title, request.Request.Description, request.Request.MetricCode,
                request.Request.WorkoutType, request.Request.Mode, request.Request.TargetValue, request.Request.Visibility,
                request.Request.StartDate.ToUniversalTime(), request.Request.EndDate.ToUniversalTime(), request.Request.JoinClosing,
                request.Request.ParticipantLimit, activeCount, DateTime.UtcNow);

            if (wasInviteOnly && challenge.Visibility != ChallengeVisibilities.InviteOnly)
                foreach (var invitation in challenge.Invitations.Where(item => item.Status == ChallengeInvitationStatuses.Pending))
                    invitation.Cancel(DateTime.UtcNow);

            await NotifyChallengeUpdatedAsync(challenge, request.UserId, pendingInviteeIds, token);
            await _context.SaveChangesAsync(token);
        }, cancellationToken);

    private async Task NotifyChallengeUpdatedAsync(UserChallenge challenge, Guid actorUserId, IEnumerable<Guid> existingPendingInviteeIds,
        CancellationToken cancellationToken)
    {
        var recipientIds = challenge.Participants.Where(item => item.Status == ChallengeParticipantStatuses.Active && item.UserId != actorUserId)
            .Select(item => item.UserId)
            .Concat(challenge.Invitations.Where(item => item.Status == ChallengeInvitationStatuses.Pending).Select(item => item.InvitedUserId))
            .Concat(existingPendingInviteeIds)
            .Distinct();

        foreach (var recipientId in recipientIds)
            await _notifications.CreateAsync(recipientId, NotificationType.ChallengeUpdated, $"{challenge.Title} was updated.", actorUserId,
                challenge.Id, NotificationReferenceTypes.Challenge, cancellationToken);
    }
}

public class CancelChallengeHandler : IRequestHandler<CancelChallengeCommand>
{
    private readonly FitspireDbContext _context;
    private readonly IChallengeTransactionService _transactions;
    private readonly INotificationService _notifications;

    public CancelChallengeHandler(FitspireDbContext context, IChallengeTransactionService transactions, INotificationService notifications)
    {
        _context = context;
        _transactions = transactions;
        _notifications = notifications;
    }

    public Task Handle(CancelChallengeCommand request, CancellationToken cancellationToken) =>
        _transactions.ExecuteAsync(async token =>
        {
            var challenge = await _context.Challenges.Include(item => item.Participants).Include(item => item.Invitations)
                .FirstOrDefaultAsync(item => item.Id == request.ChallengeId, token) ?? throw new NotFoundException("Challenge not found.");
            if (challenge.CreatedBy != request.UserId) throw new UnauthorizedAccessException("Only the creator can cancel a challenge.");
            if (challenge.Status == ChallengeStatuses.Cancelled) return;
            if (challenge.Status is not (ChallengeStatuses.Upcoming or ChallengeStatuses.Active))
                throw new DomainException("Only upcoming or active challenges can be cancelled.");

            var nowUtc = DateTime.UtcNow;
            challenge.Cancel(nowUtc);
            foreach (var invitation in challenge.Invitations.Where(item => item.Status == ChallengeInvitationStatuses.Pending)) invitation.Expire(nowUtc);
            foreach (var userId in challenge.Participants.Where(item => item.Status == ChallengeParticipantStatuses.Active && item.UserId != request.UserId).Select(item => item.UserId))
                await _notifications.CreateAsync(userId, NotificationType.ChallengeCancelled, $"{challenge.Title} was cancelled.", request.UserId,
                    challenge.Id, NotificationReferenceTypes.Challenge, token);
            await _context.SaveChangesAsync(token);
        }, cancellationToken);
}

public class RemoveChallengeParticipantHandler : IRequestHandler<RemoveChallengeParticipantCommand>
{
    private readonly FitspireDbContext _context;
    private readonly IChallengeTransactionService _transactions;

    public RemoveChallengeParticipantHandler(FitspireDbContext context, IChallengeTransactionService transactions)
    {
        _context = context;
        _transactions = transactions;
    }

    public Task Handle(RemoveChallengeParticipantCommand request, CancellationToken cancellationToken) =>
        _transactions.ExecuteAsync(async token =>
        {
            var challenge = await _context.Challenges.Include(item => item.Participants)
                .FirstOrDefaultAsync(item => item.Id == request.ChallengeId, token) ?? throw new NotFoundException("Challenge not found.");
            if (challenge.CreatedBy != request.UserId || challenge.Status != ChallengeStatuses.Upcoming)
                throw new UnauthorizedAccessException("Only the creator can remove participants before the challenge starts.");
            if (request.ParticipantUserId == challenge.CreatedBy) throw new DomainException("The challenge creator cannot be removed.");

            var participant = challenge.Participants.SingleOrDefault(item => item.UserId == request.ParticipantUserId)
                ?? throw new NotFoundException("Challenge participant not found.");
            participant.Remove(DateTime.UtcNow);
            await _context.SaveChangesAsync(token);
        }, cancellationToken);
}
