using backend.Data;
using backend.Modules.Shared.Domain;
using backend.Modules.Social.Contracts.Profiles;
using backend.Modules.Social.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.Social.Features.Profiles;

public record GetPublicGoalsQuery(Guid ViewerId, Guid OwnerId) : IRequest<List<PublicGoalResponse>>;
public record GetPublicBadgesQuery(Guid ViewerId, Guid OwnerId, bool FeaturedOnly) : IRequest<List<PublicBadgeResponse>>;
public record GetPublicChallengeResultsQuery(Guid ViewerId, Guid OwnerId) : IRequest<List<PublicChallengeResultResponse>>;

public class GetPublicGoalsHandler : IRequestHandler<GetPublicGoalsQuery, List<PublicGoalResponse>>
{
    private readonly FitspireDbContext _context; private readonly ISocialAccessService _access;
    public GetPublicGoalsHandler(FitspireDbContext context, ISocialAccessService access) { _context = context; _access = access; }
    public async Task<List<PublicGoalResponse>> Handle(GetPublicGoalsQuery request, CancellationToken cancellationToken)
    {
        if (!await _access.CanViewProtectedContentAsync(request.ViewerId, request.OwnerId, cancellationToken)) throw new UnauthorizedAccessException("This profile is private.");
        var query = _context.Goals.Include(goal => goal.GoalType).Where(goal => goal.UserId == request.OwnerId);
        if (request.ViewerId != request.OwnerId) query = query.Where(goal => goal.IsPublic);
        return await query.OrderByDescending(goal => goal.CreatedAt).Select(goal => new PublicGoalResponse(goal.Id, goal.GoalType.Name, goal.TargetValue, goal.CurrentValue, goal.Unit, goal.Status.ToString(), goal.IsRecurring, goal.CreatedAt)).ToListAsync(cancellationToken);
    }
}
public class GetPublicBadgesHandler : IRequestHandler<GetPublicBadgesQuery, List<PublicBadgeResponse>>
{
    private readonly FitspireDbContext _context; private readonly ISocialAccessService _access;
    public GetPublicBadgesHandler(FitspireDbContext context, ISocialAccessService access) { _context = context; _access = access; }
    public async Task<List<PublicBadgeResponse>> Handle(GetPublicBadgesQuery request, CancellationToken cancellationToken)
    {
        if (!await _access.CanViewProtectedContentAsync(request.ViewerId, request.OwnerId, cancellationToken)) throw new UnauthorizedAccessException("This profile is private.");
        var query = _context.UserBadges.Include(award => award.AchievementBadge).Where(award => award.UserId == request.OwnerId);
        if (request.FeaturedOnly) query = query.Where(award => award.FeaturedOrder != null).OrderBy(award => award.FeaturedOrder);
        return await query.OrderByDescending(award => award.AwardedAt).Select(award => new PublicBadgeResponse(award.BadgeId, award.AchievementBadge.Code, award.AchievementBadge.Name, award.AchievementBadge.Description, award.AchievementBadge.Tier, award.AwardedAt, award.FeaturedOrder)).ToListAsync(cancellationToken);
    }
}
public class GetPublicChallengeResultsHandler : IRequestHandler<GetPublicChallengeResultsQuery, List<PublicChallengeResultResponse>>
{
    private readonly FitspireDbContext _context; private readonly ISocialAccessService _access;
    public GetPublicChallengeResultsHandler(FitspireDbContext context, ISocialAccessService access) { _context = context; _access = access; }
    public async Task<List<PublicChallengeResultResponse>> Handle(GetPublicChallengeResultsQuery request, CancellationToken cancellationToken)
    {
        if (!await _access.CanViewProtectedContentAsync(request.ViewerId, request.OwnerId, cancellationToken)) throw new UnauthorizedAccessException("This profile is private.");
        return await _context.ChallengeResults.Include(result => result.Challenge).Where(result => result.UserId == request.OwnerId).OrderByDescending(result => result.FinalizedAt)
            .Select(result => new PublicChallengeResultResponse(result.ChallengeId, result.Challenge.Title, result.Challenge.Mode, result.Score, result.Rank, result.IsFinisher, result.IsWinner, result.FinalizedAt)).ToListAsync(cancellationToken);
    }
}
