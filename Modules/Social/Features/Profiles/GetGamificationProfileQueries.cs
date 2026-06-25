using backend.Data;
using backend.Modules.Badge.Contracts;
using backend.Modules.Goal.Domain.Enums;
using backend.Modules.Goal.DTOs;
using backend.Modules.Shared.Domain;
using backend.Modules.Social.Contracts.Profiles;
using backend.Modules.Social.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace backend.Modules.Social.Features.Profiles;

public record GetPublicGoalsQuery(Guid ViewerId, Guid OwnerId) : IRequest<List<PublicGoalResponse>>;
public record GetPublicGoalPeriodsQuery(Guid ViewerId, Guid OwnerId, int Page, int PageSize) : IRequest<GoalPageResponse<PublicGoalPeriodResponse>>;
public record GetPublicBadgesQuery(Guid ViewerId, Guid OwnerId, PublicBadgeFilter Filter) : IRequest<BadgePageResponse<PublicBadgeResponse>>;
public record GetFeaturedPublicBadgesQuery(Guid ViewerId, Guid OwnerId) : IRequest<IReadOnlyList<PublicBadgeResponse>>;
public record GetPublicChallengeResultsQuery(Guid ViewerId, Guid OwnerId) : IRequest<List<PublicChallengeResultResponse>>;

public class GetPublicGoalsHandler : IRequestHandler<GetPublicGoalsQuery, List<PublicGoalResponse>>
{
    private readonly FitspireDbContext _context; private readonly ISocialAccessService _access;
    public GetPublicGoalsHandler(FitspireDbContext context, ISocialAccessService access) { _context = context; _access = access; }
    public async Task<List<PublicGoalResponse>> Handle(GetPublicGoalsQuery request, CancellationToken cancellationToken)
    {
        if (!await _access.CanViewProtectedContentAsync(request.ViewerId, request.OwnerId, cancellationToken)) throw new UnauthorizedAccessException("This profile is private.");
        var query = _context.Goals.Include(goal => goal.GoalType).Where(goal => goal.UserId == request.OwnerId);
        if (request.ViewerId != request.OwnerId)
            query = query.Where(goal => goal.IsPublic && (goal.Status == GoalStatus.Active || goal.Status == GoalStatus.Completed));
        return await query.OrderByDescending(goal => goal.CreatedAt).Select(goal => new PublicGoalResponse(goal.Id, goal.GoalType.Name, goal.TargetValue, goal.CurrentValue, goal.Unit, goal.Status.ToString(), goal.IsRecurring, goal.CreatedAt)).ToListAsync(cancellationToken);
    }
}
public class GetPublicBadgesHandler : IRequestHandler<GetPublicBadgesQuery, BadgePageResponse<PublicBadgeResponse>>
{
    private readonly FitspireDbContext _context; private readonly ISocialAccessService _access;
    public GetPublicBadgesHandler(FitspireDbContext context, ISocialAccessService access) { _context = context; _access = access; }
    public async Task<BadgePageResponse<PublicBadgeResponse>> Handle(GetPublicBadgesQuery request, CancellationToken cancellationToken)
    {
        if (!await _access.CanViewProtectedContentAsync(request.ViewerId, request.OwnerId, cancellationToken)) throw new UnauthorizedAccessException("This profile is private.");
        var query = PublicBadgeResponseFactory.CreateQuery(_context, request.OwnerId, request.Filter.Category);
        var totalCount = await query.CountAsync(cancellationToken);
        var badges = await query.OrderByDescending(award => award.AwardedAt).ThenBy(award => award.Id)
            .Skip((request.Filter.Page - 1) * request.Filter.PageSize).Take(request.Filter.PageSize)
            .Select(PublicBadgeResponseFactory.Projection()).ToListAsync(cancellationToken);
        return new BadgePageResponse<PublicBadgeResponse>(badges, request.Filter.Page, request.Filter.PageSize, totalCount);
    }
}

public class GetPublicGoalPeriodsHandler : IRequestHandler<GetPublicGoalPeriodsQuery, GoalPageResponse<PublicGoalPeriodResponse>>
{
    private readonly FitspireDbContext _context;
    private readonly ISocialAccessService _access;

    public GetPublicGoalPeriodsHandler(FitspireDbContext context, ISocialAccessService access)
    {
        _context = context;
        _access = access;
    }

    public async Task<GoalPageResponse<PublicGoalPeriodResponse>> Handle(GetPublicGoalPeriodsQuery request,
        CancellationToken cancellationToken)
    {
        if (!await _access.CanViewProtectedContentAsync(request.ViewerId, request.OwnerId, cancellationToken))
            throw new UnauthorizedAccessException("This profile is private.");

        var query = _context.GoalPeriods.AsNoTracking().Where(period => period.Goal.UserId == request.OwnerId &&
            period.Goal.IsPublic && period.Goal.IsRecurring && period.Status == "Completed");
        var totalCount = await query.CountAsync(cancellationToken);
        var periods = await query.OrderByDescending(period => period.CompletedAt).ThenByDescending(period => period.Id)
            .Skip((request.Page - 1) * request.PageSize).Take(request.PageSize)
            .Select(period => new PublicGoalPeriodResponse(period.GoalId, period.Goal.GoalType.Name, period.StartAt,
                period.EndAt, period.TargetValue, period.ProgressValue, period.CompletedAt!.Value))
            .ToListAsync(cancellationToken);
        return new GoalPageResponse<PublicGoalPeriodResponse>(periods, request.Page, request.PageSize, totalCount);
    }
}

public class GetFeaturedPublicBadgesHandler : IRequestHandler<GetFeaturedPublicBadgesQuery, IReadOnlyList<PublicBadgeResponse>>
{
    private readonly FitspireDbContext _context;
    private readonly ISocialAccessService _access;

    public GetFeaturedPublicBadgesHandler(FitspireDbContext context, ISocialAccessService access)
    {
        _context = context;
        _access = access;
    }

    public async Task<IReadOnlyList<PublicBadgeResponse>> Handle(GetFeaturedPublicBadgesQuery request, CancellationToken cancellationToken)
    {
        if (!await _access.CanViewProtectedContentAsync(request.ViewerId, request.OwnerId, cancellationToken))
            throw new UnauthorizedAccessException("This profile is private.");

        return await PublicBadgeResponseFactory.CreateQuery(_context, request.OwnerId, null).Where(award => award.FeaturedOrder != null)
            .OrderBy(award => award.FeaturedOrder).ThenBy(award => award.BadgeId).Take(5)
            .Select(PublicBadgeResponseFactory.Projection()).ToListAsync(cancellationToken);
    }
}

internal static class PublicBadgeResponseFactory
{
    public static IQueryable<backend.Modules.Badge.Domain.UserBadge> CreateQuery(FitspireDbContext context, Guid ownerId,
        string? category)
    {
        var query = context.UserBadges.AsNoTracking().Where(award => award.UserId == ownerId);
        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(award => award.AchievementBadge.Category == category);
        return query;
    }

    public static Expression<Func<backend.Modules.Badge.Domain.UserBadge, PublicBadgeResponse>> Projection() => award =>
        new PublicBadgeResponse(award.BadgeId, award.AchievementBadge.Code, award.AchievementBadge.Name,
            award.AchievementBadge.Description, award.AchievementBadge.IconUrl, award.AchievementBadge.Category,
            award.AchievementBadge.SeriesCode, award.AchievementBadge.Tier, award.AwardedAt, award.FeaturedOrder,
            new PublicBadgeEvidenceResponse(award.CriterionCode, award.ThresholdSnapshot, award.AchievedValue,
                award.CanonicalUnit, award.EvidenceSummary));
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
