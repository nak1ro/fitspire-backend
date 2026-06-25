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

public record JoinChallengeCommand(Guid UserId, Guid ChallengeId) : IRequest;
public record LeaveChallengeCommand(Guid UserId, Guid ChallengeId) : IRequest;
public record InviteChallengeUserCommand(Guid UserId, Guid ChallengeId, Guid InvitedUserId) : IRequest;
public record RespondChallengeInvitationCommand(Guid UserId, Guid InvitationId, bool Accept) : IRequest;
public record CancelChallengeInvitationCommand(Guid UserId, Guid InvitationId) : IRequest;

public class JoinChallengeHandler : IRequestHandler<JoinChallengeCommand>
{
    private readonly FitspireDbContext _context;
    private readonly IChallengeTransactionService _transactions;
    private readonly IChallengeScoringService _scoring;

    public JoinChallengeHandler(FitspireDbContext context, IChallengeTransactionService transactions, IChallengeScoringService scoring)
    {
        _context = context;
        _transactions = transactions;
        _scoring = scoring;
    }

    public Task Handle(JoinChallengeCommand request, CancellationToken cancellationToken) =>
        _transactions.ExecuteAsync(token => JoinAsync(request.UserId, request.ChallengeId, token), cancellationToken);

    private async Task JoinAsync(Guid userId, Guid challengeId, CancellationToken cancellationToken)
    {
        var challenge = await _context.Challenges.Include(item => item.Participants).FirstOrDefaultAsync(item => item.Id == challengeId, cancellationToken)
            ?? throw new NotFoundException("Challenge not found.");
        var nowUtc = DateTime.UtcNow;
        if (!challenge.IsJoinOpen(nowUtc)) throw new DomainException("This challenge can no longer be joined.");

        var current = challenge.Participants.SingleOrDefault(item => item.UserId == userId);
        if (current?.Status == ChallengeParticipantStatuses.Active) return;
        if (challenge.Participants.Count(item => item.Status == ChallengeParticipantStatuses.Active) >= challenge.ParticipantLimit)
            throw new DomainException("This challenge is full.");

        await EnsureJoinAccessAsync(challenge, userId, cancellationToken);
        if (current is null)
            await _context.ChallengeParticipants.AddAsync(ChallengeParticipant.Create(challenge.Id, userId, nowUtc), cancellationToken);
        else
            current.Reactivate(nowUtc);

        await _context.SaveChangesAsync(cancellationToken);
        await _scoring.RecalculateForUserAsync(userId, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureJoinAccessAsync(UserChallenge challenge, Guid userId, CancellationToken cancellationToken)
    {
        if (challenge.Visibility == ChallengeVisibilities.Public) return;
        if (challenge.Visibility == ChallengeVisibilities.FollowersOnly)
        {
            var followsCreator = await _context.Followers.AnyAsync(item => item.FollowerId == userId && item.FollowedId == challenge.CreatedBy, cancellationToken);
            if (followsCreator) return;
            throw new UnauthorizedAccessException("Only followers of the creator can join.");
        }

        var acceptedInvitation = await _context.ChallengeInvitations.AnyAsync(item => item.ChallengeId == challenge.Id &&
            item.InvitedUserId == userId && item.Status == ChallengeInvitationStatuses.Accepted, cancellationToken);
        if (acceptedInvitation) return;
        throw new UnauthorizedAccessException("An accepted invitation is required to join this challenge.");
    }
}

public class LeaveChallengeHandler : IRequestHandler<LeaveChallengeCommand>
{
    private readonly FitspireDbContext _context;
    private readonly IChallengeTransactionService _transactions;

    public LeaveChallengeHandler(FitspireDbContext context, IChallengeTransactionService transactions)
    {
        _context = context;
        _transactions = transactions;
    }

    public Task Handle(LeaveChallengeCommand request, CancellationToken cancellationToken) =>
        _transactions.ExecuteAsync(async token =>
        {
            var participant = await _context.ChallengeParticipants.Include(item => item.UserChallenge)
                .FirstOrDefaultAsync(item => item.ChallengeId == request.ChallengeId && item.UserId == request.UserId, token)
                ?? throw new NotFoundException("Challenge membership not found.");
            if (participant.UserChallenge.CreatedBy == request.UserId) throw new DomainException("The creator cannot leave their own challenge.");
            if (participant.UserChallenge.Status is ChallengeStatuses.Completed or ChallengeStatuses.Cancelled)
                throw new DomainException("Completed or cancelled challenges cannot be changed.");

            participant.Leave(DateTime.UtcNow);
            await _context.SaveChangesAsync(token);
        }, cancellationToken);
}

public class InviteChallengeUserHandler : IRequestHandler<InviteChallengeUserCommand>
{
    private readonly FitspireDbContext _context;
    private readonly IChallengeTransactionService _transactions;
    private readonly INotificationService _notifications;

    public InviteChallengeUserHandler(FitspireDbContext context, IChallengeTransactionService transactions, INotificationService notifications)
    {
        _context = context;
        _transactions = transactions;
        _notifications = notifications;
    }

    public Task Handle(InviteChallengeUserCommand request, CancellationToken cancellationToken) =>
        _transactions.ExecuteAsync(async token =>
        {
            var challenge = await _context.Challenges.Include(item => item.Participants).Include(item => item.Invitations)
                .FirstOrDefaultAsync(item => item.Id == request.ChallengeId, token) ?? throw new NotFoundException("Challenge not found.");
            if (challenge.CreatedBy != request.UserId || challenge.Status != ChallengeStatuses.Upcoming)
                throw new UnauthorizedAccessException("Only the creator can invite before the challenge starts.");
            if (request.InvitedUserId == request.UserId) throw new DomainException("You cannot invite yourself.");
            if (challenge.Participants.Any(item => item.UserId == request.InvitedUserId && item.Status == ChallengeParticipantStatuses.Active))
                throw new DomainException("This user has already joined the challenge.");
            if (challenge.Participants.Count(item => item.Status == ChallengeParticipantStatuses.Active) >= challenge.ParticipantLimit)
                throw new DomainException("This challenge is full.");

            var connected = await _context.Followers.AnyAsync(item =>
                (item.FollowerId == request.UserId && item.FollowedId == request.InvitedUserId) ||
                (item.FollowerId == request.InvitedUserId && item.FollowedId == request.UserId), token);
            if (!connected) throw new DomainException("Challenges can only be sent to a follower or followed user.");

            var invitation = challenge.Invitations.SingleOrDefault(item => item.InvitedUserId == request.InvitedUserId);
            var shouldNotify = false;
            if (invitation is null)
            {
                invitation = ChallengeInvitation.Create(challenge.Id, request.InvitedUserId, request.UserId, DateTime.UtcNow);
                await _context.ChallengeInvitations.AddAsync(invitation, token);
                shouldNotify = true;
            }
            else if (invitation.Status != ChallengeInvitationStatuses.Pending)
            {
                invitation.Reopen(DateTime.UtcNow);
                shouldNotify = true;
            }

            if (shouldNotify)
                await _notifications.CreateAsync(request.InvitedUserId, NotificationType.ChallengeInvitation, $"You were invited to {challenge.Title}.", request.UserId,
                    challenge.Id, NotificationReferenceTypes.Challenge, token);
            await _context.SaveChangesAsync(token);
        }, cancellationToken);
}

public class RespondChallengeInvitationHandler : IRequestHandler<RespondChallengeInvitationCommand>
{
    private readonly FitspireDbContext _context;
    private readonly IChallengeTransactionService _transactions;
    private readonly IChallengeScoringService _scoring;

    public RespondChallengeInvitationHandler(FitspireDbContext context, IChallengeTransactionService transactions, IChallengeScoringService scoring)
    {
        _context = context;
        _transactions = transactions;
        _scoring = scoring;
    }

    public Task Handle(RespondChallengeInvitationCommand request, CancellationToken cancellationToken) =>
        _transactions.ExecuteAsync(async token =>
        {
            var invitation = await _context.ChallengeInvitations.Include(item => item.Challenge).ThenInclude(item => item.Participants)
                .FirstOrDefaultAsync(item => item.Id == request.InvitationId && item.InvitedUserId == request.UserId, token)
                ?? throw new NotFoundException("Invitation not found.");
            if (invitation.Status != ChallengeInvitationStatuses.Pending) return;

            var nowUtc = DateTime.UtcNow;
            if (!request.Accept)
            {
                invitation.Reject(nowUtc);
                await _context.SaveChangesAsync(token);
                return;
            }

            var challenge = invitation.Challenge;
            if (!challenge.IsJoinOpen(nowUtc))
            {
                invitation.Expire(nowUtc);
                await _context.SaveChangesAsync(token);
                throw new DomainException("This challenge can no longer be joined.");
            }

            var current = challenge.Participants.SingleOrDefault(item => item.UserId == request.UserId);
            if (current?.Status != ChallengeParticipantStatuses.Active &&
                challenge.Participants.Count(item => item.Status == ChallengeParticipantStatuses.Active) >= challenge.ParticipantLimit)
                throw new DomainException("This challenge is full.");

            invitation.Accept(nowUtc);
            if (current is null)
                await _context.ChallengeParticipants.AddAsync(ChallengeParticipant.Create(challenge.Id, request.UserId, nowUtc), token);
            else if (current.Status != ChallengeParticipantStatuses.Active)
                current.Reactivate(nowUtc);

            await _context.SaveChangesAsync(token);
            await _scoring.RecalculateForUserAsync(request.UserId, token);
            await _context.SaveChangesAsync(token);
        }, cancellationToken);
}

public class CancelChallengeInvitationHandler : IRequestHandler<CancelChallengeInvitationCommand>
{
    private readonly FitspireDbContext _context;
    private readonly IChallengeTransactionService _transactions;

    public CancelChallengeInvitationHandler(FitspireDbContext context, IChallengeTransactionService transactions)
    {
        _context = context;
        _transactions = transactions;
    }

    public Task Handle(CancelChallengeInvitationCommand request, CancellationToken cancellationToken) =>
        _transactions.ExecuteAsync(async token =>
        {
            var invitation = await _context.ChallengeInvitations.Include(item => item.Challenge)
                .FirstOrDefaultAsync(item => item.Id == request.InvitationId, token) ?? throw new NotFoundException("Invitation not found.");
            if (invitation.Challenge.CreatedBy != request.UserId) throw new UnauthorizedAccessException("Only the creator can cancel an invitation.");
            invitation.Cancel(DateTime.UtcNow);
            await _context.SaveChangesAsync(token);
        }, cancellationToken);
}
