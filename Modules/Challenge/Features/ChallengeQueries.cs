using AutoMapper;
using backend.Data;
using backend.Modules.Challenge.Contracts;
using backend.Modules.Challenge.Domain;
using backend.Modules.Challenge.Domain.Constants;
using backend.Modules.Challenge.Services;
using backend.Modules.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.Challenge.Features;

public record GetChallengeQuery(Guid UserId, Guid ChallengeId) : IRequest<ChallengeDetailResponse>;
public record DiscoverChallengesQuery(Guid UserId, int Page, int PageSize) : IRequest<ChallengePageResponse<ChallengeResponse>>;
public record GetAvailableChallengesQuery(Guid UserId, int Page, int PageSize) : IRequest<ChallengePageResponse<ChallengeResponse>>;
public record GetMyChallengesQuery(Guid UserId, ChallengeListFilter Filter) : IRequest<ChallengePageResponse<ChallengeResponse>>;
public record GetChallengeLeaderboardQuery(Guid UserId, Guid ChallengeId, int Page, int PageSize) : IRequest<ChallengePageResponse<ChallengeLeaderboardEntry>>;
public record GetChallengeResultsQuery(Guid UserId, Guid ChallengeId, int Page, int PageSize) : IRequest<ChallengePageResponse<ChallengeLeaderboardEntry>>;
public record GetIncomingChallengeInvitationsQuery(Guid UserId, int Page, int PageSize) : IRequest<ChallengePageResponse<ChallengeInvitationResponse>>;

public class GetChallengeHandler : IRequestHandler<GetChallengeQuery, ChallengeDetailResponse>
{
    private readonly FitspireDbContext _context;
    private readonly IChallengeAccessService _access;
    private readonly IMapper _mapper;

    public GetChallengeHandler(FitspireDbContext context, IChallengeAccessService access, IMapper mapper)
    {
        _context = context;
        _access = access;
        _mapper = mapper;
    }

    public async Task<ChallengeDetailResponse> Handle(GetChallengeQuery request, CancellationToken cancellationToken)
    {
        var challenge = await _context.Challenges.Include(item => item.CreatedByUser).Include(item => item.Participants)
            .FirstOrDefaultAsync(item => item.Id == request.ChallengeId, cancellationToken) ?? throw new NotFoundException("Challenge not found.");
        await _access.EnsureCanViewAsync(challenge, request.UserId, cancellationToken);
        var participant = challenge.Participants.SingleOrDefault(item => item.UserId == request.UserId);
        var viewer = new ChallengeViewerState(challenge.CreatedBy == request.UserId, participant?.Status,
            participant?.Score, ChallengeResponseFactory.ProgressPercent(challenge, participant?.Score),
            await _access.CanJoinAsync(challenge, request.UserId, cancellationToken), challenge.CreatedBy == request.UserId && challenge.Status == ChallengeStatuses.Upcoming);

        return new ChallengeDetailResponse(challenge.Id, challenge.Title, challenge.Description, challenge.MetricCode,
            challenge.WorkoutType, challenge.Mode, challenge.TargetValue, challenge.Visibility, challenge.StartDate,
            challenge.EndDate, challenge.JoinClosing, challenge.ParticipantLimit, challenge.Status,
            _mapper.Map<ChallengeCreatorResponse>(challenge.CreatedByUser),
            challenge.Participants.Count(item => item.Status == ChallengeParticipantStatuses.Active), viewer);
    }
}

public class DiscoverChallengesHandler : IRequestHandler<DiscoverChallengesQuery, ChallengePageResponse<ChallengeResponse>>
{
    private readonly FitspireDbContext _context;
    public DiscoverChallengesHandler(FitspireDbContext context) => _context = context;

    public Task<ChallengePageResponse<ChallengeResponse>> Handle(DiscoverChallengesQuery request, CancellationToken cancellationToken) =>
        ChallengeResponseFactory.CreatePageAsync(_context.Challenges.Include(item => item.Participants)
            .Where(item => item.Visibility == ChallengeVisibilities.Public && (item.Status == ChallengeStatuses.Upcoming || item.Status == ChallengeStatuses.Active))
            .OrderBy(item => item.StartDate), request.UserId, request.Page, request.PageSize, cancellationToken);
}

public class GetAvailableChallengesHandler : IRequestHandler<GetAvailableChallengesQuery, ChallengePageResponse<ChallengeResponse>>
{
    private readonly FitspireDbContext _context;
    public GetAvailableChallengesHandler(FitspireDbContext context) => _context = context;

    public Task<ChallengePageResponse<ChallengeResponse>> Handle(GetAvailableChallengesQuery request, CancellationToken cancellationToken) =>
        ChallengeResponseFactory.CreatePageAsync(_context.Challenges.Include(item => item.Participants)
            .Where(item => item.Visibility == ChallengeVisibilities.FollowersOnly && (item.Status == ChallengeStatuses.Upcoming || item.Status == ChallengeStatuses.Active) &&
                _context.Followers.Any(follower => follower.FollowerId == request.UserId && follower.FollowedId == item.CreatedBy) &&
                !item.Participants.Any(participant => participant.UserId == request.UserId && participant.Status == ChallengeParticipantStatuses.Active))
            .OrderBy(item => item.StartDate), request.UserId, request.Page, request.PageSize, cancellationToken);
}

public class GetMyChallengesHandler : IRequestHandler<GetMyChallengesQuery, ChallengePageResponse<ChallengeResponse>>
{
    private readonly FitspireDbContext _context;
    public GetMyChallengesHandler(FitspireDbContext context) => _context = context;

    public Task<ChallengePageResponse<ChallengeResponse>> Handle(GetMyChallengesQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter;
        var query = _context.Challenges.Include(item => item.Participants).AsQueryable();
        query = filter.Role switch
        {
            "Created" => query.Where(item => item.CreatedBy == request.UserId),
            "Joined" => query.Where(item => item.CreatedBy != request.UserId && item.Participants.Any(participant => participant.UserId == request.UserId)),
            _ => query.Where(item => item.CreatedBy == request.UserId || item.Participants.Any(participant => participant.UserId == request.UserId))
        };
        if (!string.IsNullOrWhiteSpace(filter.Status)) query = query.Where(item => item.Status == filter.Status);
        if (!string.IsNullOrWhiteSpace(filter.MetricCode)) query = query.Where(item => item.MetricCode == filter.MetricCode);
        return ChallengeResponseFactory.CreatePageAsync(query.OrderByDescending(item => item.StartDate), request.UserId, filter.Page, filter.PageSize, cancellationToken);
    }
}

public class GetChallengeLeaderboardHandler : IRequestHandler<GetChallengeLeaderboardQuery, ChallengePageResponse<ChallengeLeaderboardEntry>>
{
    private readonly FitspireDbContext _context;
    private readonly IChallengeAccessService _access;

    public GetChallengeLeaderboardHandler(FitspireDbContext context, IChallengeAccessService access)
    {
        _context = context;
        _access = access;
    }

    public async Task<ChallengePageResponse<ChallengeLeaderboardEntry>> Handle(GetChallengeLeaderboardQuery request, CancellationToken cancellationToken)
    {
        var challenge = await _context.Challenges.FindAsync([request.ChallengeId], cancellationToken) ?? throw new NotFoundException("Challenge not found.");
        await _access.EnsureCanViewAsync(challenge, request.UserId, cancellationToken);
        var participants = _context.ChallengeParticipants.Include(item => item.User).Where(item => item.ChallengeId == challenge.Id && item.Status == ChallengeParticipantStatuses.Active)
            .OrderByDescending(item => item.Score).ThenBy(item => item.JoinedAt);
        return await CreateLeaderboardPageAsync(participants, challenge, request.Page, request.PageSize, cancellationToken);
    }

    internal static async Task<ChallengePageResponse<ChallengeLeaderboardEntry>> CreateLeaderboardPageAsync(IQueryable<ChallengeParticipant> participants,
        UserChallenge challenge, int page, int pageSize, CancellationToken cancellationToken)
    {
        var totalCount = await participants.CountAsync(cancellationToken);
        var ranked = await participants.ToListAsync(cancellationToken);
        var rank = 0;
        var previousScore = float.NaN;
        var rows = ranked.Select((participant, index) =>
        {
            if (participant.Score != previousScore) rank = index + 1;
            previousScore = participant.Score;
            return new ChallengeLeaderboardEntry(participant.UserId, participant.User.DisplayName, participant.User.ProfilePictureUrl,
                participant.Score, rank, ChallengeResponseFactory.ProgressPercent(challenge, participant.Score));
        }).Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return new ChallengePageResponse<ChallengeLeaderboardEntry>(rows, page, pageSize, totalCount);
    }
}

public class GetChallengeResultsHandler : IRequestHandler<GetChallengeResultsQuery, ChallengePageResponse<ChallengeLeaderboardEntry>>
{
    private readonly FitspireDbContext _context;
    private readonly IChallengeAccessService _access;

    public GetChallengeResultsHandler(FitspireDbContext context, IChallengeAccessService access)
    {
        _context = context;
        _access = access;
    }

    public async Task<ChallengePageResponse<ChallengeLeaderboardEntry>> Handle(GetChallengeResultsQuery request, CancellationToken cancellationToken)
    {
        var challenge = await _context.Challenges.FindAsync([request.ChallengeId], cancellationToken) ?? throw new NotFoundException("Challenge not found.");
        await _access.EnsureCanViewAsync(challenge, request.UserId, cancellationToken);
        var query = _context.ChallengeResults.Include(item => item.User).Where(item => item.ChallengeId == challenge.Id).OrderBy(item => item.Rank);
        var totalCount = await query.CountAsync(cancellationToken);
        var rows = await query.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize)
            .Select(item => new ChallengeLeaderboardEntry(item.UserId, item.User.DisplayName, item.User.ProfilePictureUrl, item.Score, item.Rank,
                challenge.Mode == ChallengeModes.Target && challenge.TargetValue.HasValue ? Math.Min(100, item.Score / challenge.TargetValue.Value * 100) : null))
            .ToListAsync(cancellationToken);
        return new ChallengePageResponse<ChallengeLeaderboardEntry>(rows, request.Page, request.PageSize, totalCount);
    }
}

public class GetIncomingChallengeInvitationsHandler : IRequestHandler<GetIncomingChallengeInvitationsQuery, ChallengePageResponse<ChallengeInvitationResponse>>
{
    private readonly FitspireDbContext _context;
    public GetIncomingChallengeInvitationsHandler(FitspireDbContext context) => _context = context;

    public async Task<ChallengePageResponse<ChallengeInvitationResponse>> Handle(GetIncomingChallengeInvitationsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.ChallengeInvitations.Include(item => item.Challenge).Include(item => item.Challenge.CreatedByUser)
            .Where(item => item.InvitedUserId == request.UserId && item.Status == ChallengeInvitationStatuses.Pending)
            .OrderByDescending(item => item.CreatedAt);
        var totalCount = await query.CountAsync(cancellationToken);
        var rows = await query.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).Select(item => new ChallengeInvitationResponse(
            item.Id, item.ChallengeId, item.Challenge.Title, item.InvitedByUserId, item.Challenge.CreatedByUser.DisplayName,
            item.Challenge.StartDate, item.Challenge.EndDate, item.Status, item.CreatedAt)).ToListAsync(cancellationToken);
        return new ChallengePageResponse<ChallengeInvitationResponse>(rows, request.Page, request.PageSize, totalCount);
    }
}

internal static class ChallengeResponseFactory
{
    public static async Task<ChallengePageResponse<ChallengeResponse>> CreatePageAsync(IQueryable<UserChallenge> query, Guid userId,
        int page, int pageSize, CancellationToken cancellationToken)
    {
        var totalCount = await query.CountAsync(cancellationToken);
        var challenges = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return new ChallengePageResponse<ChallengeResponse>(challenges.Select(item => Create(item, userId)).ToList(), page, pageSize, totalCount);
    }

    public static ChallengeResponse Create(UserChallenge challenge, Guid userId) => new(challenge.Id, challenge.Title, challenge.Description,
        challenge.MetricCode, challenge.WorkoutType, challenge.Mode, challenge.TargetValue, challenge.Visibility, challenge.StartDate,
        challenge.EndDate, challenge.JoinClosing, challenge.ParticipantLimit, challenge.Status,
        challenge.Participants.Count(item => item.Status == ChallengeParticipantStatuses.Active),
        challenge.Participants.Any(item => item.UserId == userId && item.Status == ChallengeParticipantStatuses.Active));

    public static double? ProgressPercent(UserChallenge challenge, double? score) =>
        challenge.Mode == ChallengeModes.Target && challenge.TargetValue.HasValue && score.HasValue
            ? Math.Min(100, score.Value / challenge.TargetValue.Value * 100)
            : null;
}
