using AutoMapper;
using backend.Data;
using backend.Modules.Badge.Contracts;
using backend.Modules.Badge.Domain;
using backend.Modules.Badge.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.Badge.Features;

public record GetBadgeCatalogueQuery(Guid UserId, BadgeCatalogueFilter Filter) : IRequest<BadgePageResponse<BadgeCatalogueItemResponse>>;
public record GetMyBadgesQuery(Guid UserId, BadgeCollectionFilter Filter) : IRequest<BadgePageResponse<EarnedBadgeResponse>>;

public class GetBadgeCatalogueHandler : IRequestHandler<GetBadgeCatalogueQuery, BadgePageResponse<BadgeCatalogueItemResponse>>
{
    private readonly FitspireDbContext _context;
    private readonly IBadgeAchievementSnapshotService _snapshots;
    private readonly IMapper _mapper;

    public GetBadgeCatalogueHandler(FitspireDbContext context, IBadgeAchievementSnapshotService snapshots, IMapper mapper)
    {
        _context = context;
        _snapshots = snapshots;
        _mapper = mapper;
    }

    public async Task<BadgePageResponse<BadgeCatalogueItemResponse>> Handle(GetBadgeCatalogueQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Badges.AsNoTracking().Where(badge => badge.IsActive);
        if (!string.IsNullOrWhiteSpace(request.Filter.Category))
            query = query.Where(badge => badge.Category == request.Filter.Category);
        if (request.Filter.Earned.HasValue)
            query = request.Filter.Earned.Value
                ? query.Where(badge => _context.UserBadges.Any(award => award.UserId == request.UserId && award.BadgeId == badge.Id))
                : query.Where(badge => !_context.UserBadges.Any(award => award.UserId == request.UserId && award.BadgeId == badge.Id));

        var totalCount = await query.CountAsync(cancellationToken);
        var definitions = await query.OrderBy(badge => badge.DisplayOrder).ThenBy(badge => badge.Id)
            .Skip((request.Filter.Page - 1) * request.Filter.PageSize).Take(request.Filter.PageSize)
            .ToListAsync(cancellationToken);
        var awards = await GetAwardsAsync(request.UserId, definitions, cancellationToken);
        var snapshot = await GetSnapshotIfNeededAsync(definitions, awards, request.UserId, cancellationToken);
        var items = definitions.Select(definition => CreateCatalogueItem(definition, awards.GetValueOrDefault(definition.Id), snapshot)).ToList();
        return new BadgePageResponse<BadgeCatalogueItemResponse>(items, request.Filter.Page, request.Filter.PageSize, totalCount);
    }

    private async Task<Dictionary<Guid, UserBadge>> GetAwardsAsync(Guid userId, IReadOnlyCollection<AchievementBadge> definitions,
        CancellationToken cancellationToken)
    {
        var badgeIds = definitions.Select(definition => definition.Id).ToList();
        return await _context.UserBadges.AsNoTracking().Where(award => award.UserId == userId && badgeIds.Contains(award.BadgeId))
            .ToDictionaryAsync(award => award.BadgeId, cancellationToken);
    }

    private async Task<BadgeAchievementSnapshot?> GetSnapshotIfNeededAsync(IReadOnlyCollection<AchievementBadge> definitions,
        IReadOnlyDictionary<Guid, UserBadge> awards, Guid userId, CancellationToken cancellationToken)
    {
        var needsProgress = definitions.Any(definition => definition.ShowProgressWhenLocked && !awards.ContainsKey(definition.Id));
        return needsProgress ? await _snapshots.CreateAsync(userId, cancellationToken) : null;
    }

    private BadgeCatalogueItemResponse CreateCatalogueItem(AchievementBadge definition, UserBadge? award,
        BadgeAchievementSnapshot? snapshot)
    {
        double? progress = award is null && definition.ShowProgressWhenLocked && snapshot is not null
            ? snapshot.GetValue(definition.CriterionCode)
            : null;
        double? percentage = progress.HasValue ? Math.Min(100, progress.Value / definition.Threshold * 100) : null;
        return new BadgeCatalogueItemResponse(_mapper.Map<BadgeDefinitionResponse>(definition), award is not null,
            award?.AwardedAt, award?.FeaturedOrder, progress, percentage, award is null ? null : BadgeResponseFactory.CreateEvidence(award));
    }
}

public class GetMyBadgesHandler : IRequestHandler<GetMyBadgesQuery, BadgePageResponse<EarnedBadgeResponse>>
{
    private readonly FitspireDbContext _context;
    private readonly IMapper _mapper;

    public GetMyBadgesHandler(FitspireDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<BadgePageResponse<EarnedBadgeResponse>> Handle(GetMyBadgesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.UserBadges.AsNoTracking().Include(award => award.AchievementBadge)
            .Where(award => award.UserId == request.UserId);
        if (!string.IsNullOrWhiteSpace(request.Filter.Category))
            query = query.Where(award => award.AchievementBadge.Category == request.Filter.Category);
        if (request.Filter.Featured.HasValue)
            query = request.Filter.Featured.Value
                ? query.Where(award => award.FeaturedOrder != null)
                : query.Where(award => award.FeaturedOrder == null);

        var totalCount = await query.CountAsync(cancellationToken);
        var ordered = request.Filter.Featured == true
            ? query.OrderBy(award => award.FeaturedOrder).ThenBy(award => award.BadgeId)
            : query.OrderByDescending(award => award.AwardedAt).ThenBy(award => award.Id);
        var awards = await ordered.Skip((request.Filter.Page - 1) * request.Filter.PageSize).Take(request.Filter.PageSize)
            .ToListAsync(cancellationToken);
        var items = awards.Select(award => new EarnedBadgeResponse(_mapper.Map<BadgeDefinitionResponse>(award.AchievementBadge),
            award.AwardedAt, award.FeaturedOrder, BadgeResponseFactory.CreateEvidence(award))).ToList();
        return new BadgePageResponse<EarnedBadgeResponse>(items, request.Filter.Page, request.Filter.PageSize, totalCount);
    }
}

internal static class BadgeResponseFactory
{
    public static BadgeEvidenceResponse CreateEvidence(UserBadge award) => new(award.CriterionCode, award.ThresholdSnapshot,
        award.AchievedValue, award.CanonicalUnit, award.TriggeringEntityType, award.TriggeringEntityId, award.EvidenceSummary);
}
