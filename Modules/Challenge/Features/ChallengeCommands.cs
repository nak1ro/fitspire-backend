using backend.Data;
using backend.Modules.Challenge.Contracts;
using backend.Modules.Shared.Domain;
using backend.Modules.Notification.Domain.Constants;
using backend.Modules.Notification.Domain.Enums;
using backend.Modules.Notification.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.Challenge.Features;

public record CreateChallengeCommand(Guid UserId, CreateChallengeRequest Request) : IRequest<Guid>;
public record JoinChallengeCommand(Guid UserId, Guid ChallengeId, Guid? InvitationId = null) : IRequest;
public record LeaveChallengeCommand(Guid UserId, Guid ChallengeId) : IRequest;
public record InviteChallengeUserCommand(Guid UserId, Guid ChallengeId, Guid InvitedUserId) : IRequest;
public record RespondChallengeInvitationCommand(Guid UserId, Guid InvitationId, bool Accept) : IRequest;
public record GetChallengeQuery(Guid UserId, Guid ChallengeId) : IRequest<ChallengeResponse>;
public record GetChallengeLeaderboardQuery(Guid UserId, Guid ChallengeId) : IRequest<List<ChallengeLeaderboardEntry>>;
public record DiscoverChallengesQuery(Guid UserId, int Page, int PageSize) : IRequest<List<ChallengeResponse>>;
public record GetMyChallengesQuery(Guid UserId, int Page, int PageSize) : IRequest<List<ChallengeResponse>>;
public record RemoveChallengeParticipantCommand(Guid UserId, Guid ChallengeId, Guid ParticipantUserId) : IRequest;
public record CancelChallengeCommand(Guid UserId, Guid ChallengeId) : IRequest;
public record GetChallengeResultsQuery(Guid UserId, Guid ChallengeId) : IRequest<List<ChallengeLeaderboardEntry>>;

public class CreateChallengeHandler : IRequestHandler<CreateChallengeCommand, Guid>
{
    private readonly FitspireDbContext _context; public CreateChallengeHandler(FitspireDbContext context) => _context = context;
    public async Task<Guid> Handle(CreateChallengeCommand request, CancellationToken cancellationToken)
    {
        if (!await _context.MetricDefinitions.AnyAsync(metric => metric.Id == request.Request.MetricCode && metric.IsChallengeSupported, cancellationToken))
            throw new DomainException("Challenge metric is not supported.");
        var owned = await _context.Challenges.CountAsync(item => item.CreatedBy == request.UserId && (item.Status == "Upcoming" || item.Status == "Active"), cancellationToken);
        if (owned >= 5) throw new DomainException("You can own at most five upcoming or active challenges.");
        var challenge = new Domain.UserChallenge { Id = Guid.NewGuid(), CreatedBy = request.UserId, Title = request.Request.Title.Trim(), Description = request.Request.Description?.Trim(),
            MetricCode = request.Request.MetricCode, WorkoutType = request.Request.WorkoutType?.ToLowerInvariant(), Mode = request.Request.Mode, TargetValue = request.Request.TargetValue,
            Visibility = request.Request.Visibility, StartDate = request.Request.StartDate.ToUniversalTime(), EndDate = request.Request.EndDate.ToUniversalTime(), JoinClosing = request.Request.JoinClosing,
            ParticipantLimit = request.Request.ParticipantLimit, Status = request.Request.StartDate <= DateTime.UtcNow ? "Active" : "Upcoming" };
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        await _context.Challenges.AddAsync(challenge, cancellationToken);
        await _context.ChallengeParticipants.AddAsync(new Domain.ChallengeParticipant { Id = Guid.NewGuid(), ChallengeId = challenge.Id, UserId = request.UserId, JoinedAt = DateTime.UtcNow, Status = "Active" }, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken); await transaction.CommitAsync(cancellationToken);
        return challenge.Id;
    }
}

public class JoinChallengeHandler : IRequestHandler<JoinChallengeCommand>
{
    private readonly FitspireDbContext _context; public JoinChallengeHandler(FitspireDbContext context) => _context = context;
    public async Task Handle(JoinChallengeCommand request, CancellationToken cancellationToken)
    {
        var challenge = await _context.Challenges.Include(item => item.Participants).FirstOrDefaultAsync(item => item.Id == request.ChallengeId, cancellationToken)
            ?? throw new NotFoundException("Challenge not found.");
        if (challenge.Status is not ("Upcoming" or "Active")) throw new DomainException("This challenge can no longer be joined.");
        if (challenge.JoinClosing == "AtStart" && DateTime.UtcNow >= challenge.StartDate) throw new DomainException("Joining closed when the challenge started.");
        if (challenge.Participants.Any(item => item.UserId == request.UserId && item.Status == "Active")) return;
        if (challenge.Participants.Count(item => item.Status == "Active") >= challenge.ParticipantLimit) throw new DomainException("This challenge is full.");
        if (challenge.Visibility == "FollowersOnly" && !await _context.Followers.AnyAsync(item => item.FollowerId == request.UserId && item.FollowedId == challenge.CreatedBy, cancellationToken)) throw new UnauthorizedAccessException("Only followers of the creator can join.");
        if (challenge.Visibility == "InviteOnly" && !await _context.ChallengeInvitations.AnyAsync(item => item.ChallengeId == challenge.Id && item.InvitedUserId == request.UserId && item.Status == "Accepted", cancellationToken)) throw new UnauthorizedAccessException("An accepted invitation is required.");
        var old = challenge.Participants.FirstOrDefault(item => item.UserId == request.UserId);
        if (old is not null) { old.Status = "Active"; old.LeftAt = null; old.JoinedAt = DateTime.UtcNow; old.Score = 0; } else await _context.ChallengeParticipants.AddAsync(new Domain.ChallengeParticipant { Id = Guid.NewGuid(), ChallengeId = challenge.Id, UserId = request.UserId, Status = "Active", JoinedAt = DateTime.UtcNow }, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

public class LeaveChallengeHandler : IRequestHandler<LeaveChallengeCommand>
{
    private readonly FitspireDbContext _context; public LeaveChallengeHandler(FitspireDbContext context) => _context = context;
    public async Task Handle(LeaveChallengeCommand request, CancellationToken cancellationToken)
    {
        var participant = await _context.ChallengeParticipants.Include(item => item.UserChallenge).FirstOrDefaultAsync(item => item.ChallengeId == request.ChallengeId && item.UserId == request.UserId && item.Status == "Active", cancellationToken)
            ?? throw new NotFoundException("Active challenge membership not found.");
        if (participant.UserChallenge.CreatedBy == request.UserId) throw new DomainException("The creator cannot leave their own challenge.");
        participant.Status = "Left"; participant.LeftAt = DateTime.UtcNow; await _context.SaveChangesAsync(cancellationToken);
    }
}

public class InviteChallengeUserHandler : IRequestHandler<InviteChallengeUserCommand>
{
    private readonly FitspireDbContext _context; private readonly INotificationService _notifications;
    public InviteChallengeUserHandler(FitspireDbContext context, INotificationService notifications) { _context = context; _notifications = notifications; }
    public async Task Handle(InviteChallengeUserCommand request, CancellationToken cancellationToken)
    {
        var challenge = await _context.Challenges.FindAsync([request.ChallengeId], cancellationToken) ?? throw new NotFoundException("Challenge not found.");
        if (challenge.CreatedBy != request.UserId || challenge.Status != "Upcoming") throw new UnauthorizedAccessException("Only the creator can invite before the challenge starts.");
        var connected = await _context.Followers.AnyAsync(item => (item.FollowerId == request.UserId && item.FollowedId == request.InvitedUserId) || (item.FollowerId == request.InvitedUserId && item.FollowedId == request.UserId), cancellationToken);
        if (!connected) throw new DomainException("Challenges can only be sent to a follower or followed user.");
        var invitation = await _context.ChallengeInvitations.FirstOrDefaultAsync(item => item.ChallengeId == challenge.Id && item.InvitedUserId == request.InvitedUserId, cancellationToken);
        if (invitation is null) await _context.ChallengeInvitations.AddAsync(new Domain.ChallengeInvitation { Id = Guid.NewGuid(), ChallengeId = challenge.Id, InvitedUserId = request.InvitedUserId, InvitedByUserId = request.UserId }, cancellationToken);
        else { invitation.Status = "Pending"; invitation.RespondedAt = null; }
        await _notifications.CreateAsync(request.InvitedUserId, NotificationType.ChallengeInvitation, $"You were invited to {challenge.Title}.", actorUserId: request.UserId, referenceEntityId: challenge.Id, referenceEntityType: NotificationReferenceTypes.Challenge, cancellationToken: cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

public class RespondChallengeInvitationHandler : IRequestHandler<RespondChallengeInvitationCommand>
{
    private readonly FitspireDbContext _context; private readonly IMediator _mediator;
    public RespondChallengeInvitationHandler(FitspireDbContext context, IMediator mediator) { _context = context; _mediator = mediator; }
    public async Task Handle(RespondChallengeInvitationCommand request, CancellationToken cancellationToken)
    {
        var invitation = await _context.ChallengeInvitations.FirstOrDefaultAsync(item => item.Id == request.InvitationId && item.InvitedUserId == request.UserId, cancellationToken) ?? throw new NotFoundException("Invitation not found.");
        if (invitation.Status != "Pending") return;
        invitation.Status = request.Accept ? "Accepted" : "Rejected"; invitation.RespondedAt = DateTime.UtcNow; await _context.SaveChangesAsync(cancellationToken);
        if (request.Accept) await _mediator.Send(new JoinChallengeCommand(request.UserId, invitation.ChallengeId, invitation.Id), cancellationToken);
    }
}

public class GetChallengeHandler : IRequestHandler<GetChallengeQuery, ChallengeResponse>
{
    private readonly FitspireDbContext _context; public GetChallengeHandler(FitspireDbContext context) => _context = context;
    public async Task<ChallengeResponse> Handle(GetChallengeQuery request, CancellationToken cancellationToken)
    {
        var item = await _context.Challenges.Include(challenge => challenge.Participants).FirstOrDefaultAsync(challenge => challenge.Id == request.ChallengeId, cancellationToken) ?? throw new NotFoundException("Challenge not found.");
        return Map(item, request.UserId);
    }
    internal static ChallengeResponse Map(Domain.UserChallenge item, Guid userId) => new(item.Id, item.Title, item.Description, item.MetricCode, item.WorkoutType, item.Mode, item.TargetValue, item.Visibility, item.StartDate, item.EndDate, item.JoinClosing, item.ParticipantLimit, item.Status, item.Participants.Count(participant => participant.Status == "Active"), item.Participants.Any(participant => participant.UserId == userId && participant.Status == "Active"));
}

public class GetChallengeLeaderboardHandler : IRequestHandler<GetChallengeLeaderboardQuery, List<ChallengeLeaderboardEntry>>
{
    private readonly FitspireDbContext _context; public GetChallengeLeaderboardHandler(FitspireDbContext context) => _context = context;
    public async Task<List<ChallengeLeaderboardEntry>> Handle(GetChallengeLeaderboardQuery request, CancellationToken cancellationToken)
    {
        var rows = await _context.ChallengeParticipants.Include(item => item.User).Where(item => item.ChallengeId == request.ChallengeId && item.Status == "Active").OrderByDescending(item => item.Score).ToListAsync(cancellationToken);
        var rank = 0; float last = float.NaN; return rows.Select((item, index) => { if (item.Score != last) rank = index + 1; last = item.Score; return new ChallengeLeaderboardEntry(item.UserId, item.User.DisplayName, item.Score, rank); }).ToList();
    }
}

public class DiscoverChallengesHandler : IRequestHandler<DiscoverChallengesQuery, List<ChallengeResponse>>
{
    private readonly FitspireDbContext _context; public DiscoverChallengesHandler(FitspireDbContext context) => _context = context;
    public async Task<List<ChallengeResponse>> Handle(DiscoverChallengesQuery request, CancellationToken cancellationToken) => (await _context.Challenges.Include(item => item.Participants).Where(item => item.Visibility == "Public" && (item.Status == "Upcoming" || item.Status == "Active")).OrderBy(item => item.StartDate).Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToListAsync(cancellationToken)).Select(item => GetChallengeHandler.Map(item, request.UserId)).ToList();
}

public class GetMyChallengesHandler : IRequestHandler<GetMyChallengesQuery, List<ChallengeResponse>>
{
    private readonly FitspireDbContext _context; public GetMyChallengesHandler(FitspireDbContext context) => _context = context;
    public async Task<List<ChallengeResponse>> Handle(GetMyChallengesQuery request, CancellationToken cancellationToken) => (await _context.Challenges.Include(item => item.Participants)
        .Where(item => item.Participants.Any(participant => participant.UserId == request.UserId)).OrderByDescending(item => item.StartDate).Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToListAsync(cancellationToken))
        .Select(item => GetChallengeHandler.Map(item, request.UserId)).ToList();
}

public class RemoveChallengeParticipantHandler : IRequestHandler<RemoveChallengeParticipantCommand>
{
    private readonly FitspireDbContext _context; public RemoveChallengeParticipantHandler(FitspireDbContext context) => _context = context;
    public async Task Handle(RemoveChallengeParticipantCommand request, CancellationToken cancellationToken)
    {
        var challenge = await _context.Challenges.FindAsync([request.ChallengeId], cancellationToken) ?? throw new NotFoundException("Challenge not found.");
        if (challenge.CreatedBy != request.UserId || DateTime.UtcNow >= challenge.StartDate) throw new UnauthorizedAccessException("Only the creator can remove a participant before the challenge starts.");
        var participant = await _context.ChallengeParticipants.FirstOrDefaultAsync(item => item.ChallengeId == request.ChallengeId && item.UserId == request.ParticipantUserId && item.Status == "Active", cancellationToken) ?? throw new NotFoundException("Active participant not found.");
        if (participant.UserId == challenge.CreatedBy) throw new DomainException("The challenge creator cannot be removed.");
        participant.Status = "Removed"; participant.LeftAt = DateTime.UtcNow; await _context.SaveChangesAsync(cancellationToken);
    }
}

public class CancelChallengeHandler : IRequestHandler<CancelChallengeCommand>
{
    private readonly FitspireDbContext _context; private readonly INotificationService _notifications;
    public CancelChallengeHandler(FitspireDbContext context, INotificationService notifications) { _context = context; _notifications = notifications; }
    public async Task Handle(CancelChallengeCommand request, CancellationToken cancellationToken)
    {
        var challenge = await _context.Challenges.FindAsync([request.ChallengeId], cancellationToken) ?? throw new NotFoundException("Challenge not found.");
        if (challenge.CreatedBy != request.UserId) throw new UnauthorizedAccessException("Only the creator can cancel a challenge.");
        if (challenge.Status is "Completed" or "Cancelled") return;
        challenge.Status = "Cancelled"; challenge.CancelledAt = DateTime.UtcNow;
        var users = await _context.ChallengeParticipants.Where(item => item.ChallengeId == challenge.Id && item.Status == "Active" && item.UserId != request.UserId).Select(item => item.UserId).ToListAsync(cancellationToken);
        foreach (var userId in users) await _notifications.CreateAsync(userId, NotificationType.ChallengeCancelled, $"{challenge.Title} was cancelled.", actorUserId: request.UserId, referenceEntityId: challenge.Id, referenceEntityType: NotificationReferenceTypes.Challenge, cancellationToken: cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

public class GetChallengeResultsHandler : IRequestHandler<GetChallengeResultsQuery, List<ChallengeLeaderboardEntry>>
{
    private readonly FitspireDbContext _context; public GetChallengeResultsHandler(FitspireDbContext context) => _context = context;
    public async Task<List<ChallengeLeaderboardEntry>> Handle(GetChallengeResultsQuery request, CancellationToken cancellationToken) => await _context.ChallengeResults.Include(item => item.User).Where(item => item.ChallengeId == request.ChallengeId)
        .OrderBy(item => item.Rank).Select(item => new ChallengeLeaderboardEntry(item.UserId, item.User.DisplayName, item.Score, item.Rank)).ToListAsync(cancellationToken);
}
