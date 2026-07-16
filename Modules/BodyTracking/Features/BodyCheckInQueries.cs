using AutoMapper;
using backend.Data;
using backend.Modules.BodyTracking.Contracts;
using backend.Modules.BodyTracking.Domain;
using backend.Modules.BodyTracking.Services;
using backend.Modules.Media.Contracts;
using backend.Modules.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.BodyTracking.Features;

public record GetBodyCheckInQuery(Guid UserId, Guid CheckInId) : IRequest<BodyCheckInResponse>;
public record GetBodyCheckInHistoryQuery(Guid UserId, BodyCheckInHistoryFilter Filter) : IRequest<BodyCheckInPageResponse>;
public record GetLatestBodyCheckInQuery(Guid UserId) : IRequest<BodyCheckInResponse?>;
public record GetBodyCheckInSummaryQuery(Guid UserId, BodyCheckInSummaryFilter Filter) : IRequest<BodyCheckInSummaryResponse>;

public class GetBodyCheckInHandler : IRequestHandler<GetBodyCheckInQuery, BodyCheckInResponse>
{
    private readonly FitspireDbContext _context;
    private readonly IMapper _mapper;
    private readonly IMediaResponseFactory _mediaResponseFactory;

    public GetBodyCheckInHandler(FitspireDbContext context, IMapper mapper, IMediaResponseFactory mediaResponseFactory)
    {
        _context = context;
        _mapper = mapper;
        _mediaResponseFactory = mediaResponseFactory;
    }

    public async Task<BodyCheckInResponse> Handle(GetBodyCheckInQuery query, CancellationToken cancellationToken)
    {
        var checkIn = await BodyCheckInQuerySupport.ActiveForUser(_context, query.UserId)
            .FirstOrDefaultAsync(candidate => candidate.Id == query.CheckInId, cancellationToken)
            ?? throw new NotFoundException("Body check-in was not found.");
        return await BodyCheckInQuerySupport.MapAsync(checkIn, _mapper, _mediaResponseFactory, cancellationToken);
    }
}

public class GetBodyCheckInHistoryHandler : IRequestHandler<GetBodyCheckInHistoryQuery, BodyCheckInPageResponse>
{
    private readonly FitspireDbContext _context;
    private readonly IMapper _mapper;
    private readonly IMediaResponseFactory _mediaResponseFactory;
    private readonly IBodyCheckInTimeZoneService _timeZoneService;

    public GetBodyCheckInHistoryHandler(FitspireDbContext context, IMapper mapper,
        IMediaResponseFactory mediaResponseFactory, IBodyCheckInTimeZoneService timeZoneService)
    {
        _context = context;
        _mapper = mapper;
        _mediaResponseFactory = mediaResponseFactory;
        _timeZoneService = timeZoneService;
    }

    public async Task<BodyCheckInPageResponse> Handle(GetBodyCheckInHistoryQuery query, CancellationToken cancellationToken)
    {
        var today = await _timeZoneService.GetTodayAsync(query.UserId, cancellationToken);
        if (query.Filter.From > today || query.Filter.To > today)
            throw new DomainException("Body check-in history cannot include future local dates.");

        var entries = BodyCheckInQuerySupport.ActiveForUser(_context, query.UserId);
        if (query.Filter.From.HasValue)
            entries = entries.Where(checkIn => checkIn.CheckInDate >= query.Filter.From.Value);
        if (query.Filter.To.HasValue)
            entries = entries.Where(checkIn => checkIn.CheckInDate <= query.Filter.To.Value);

        var totalCount = await entries.CountAsync(cancellationToken);
        var page = await entries.OrderByDescending(checkIn => checkIn.CheckInDate).ThenByDescending(checkIn => checkIn.Id)
            .Skip((query.Filter.Page - 1) * query.Filter.PageSize).Take(query.Filter.PageSize).ToListAsync(cancellationToken);
        var items = await BodyCheckInQuerySupport.MapManyAsync(page, _mapper, _mediaResponseFactory, cancellationToken);
        return new BodyCheckInPageResponse(items, query.Filter.Page, query.Filter.PageSize, totalCount);
    }
}

public class GetLatestBodyCheckInHandler : IRequestHandler<GetLatestBodyCheckInQuery, BodyCheckInResponse?>
{
    private readonly FitspireDbContext _context;
    private readonly IMapper _mapper;
    private readonly IMediaResponseFactory _mediaResponseFactory;

    public GetLatestBodyCheckInHandler(FitspireDbContext context, IMapper mapper, IMediaResponseFactory mediaResponseFactory)
    {
        _context = context;
        _mapper = mapper;
        _mediaResponseFactory = mediaResponseFactory;
    }

    public async Task<BodyCheckInResponse?> Handle(GetLatestBodyCheckInQuery query, CancellationToken cancellationToken)
    {
        var checkIn = await BodyCheckInQuerySupport.ActiveForUser(_context, query.UserId)
            .OrderByDescending(candidate => candidate.CheckInDate).ThenByDescending(candidate => candidate.Id)
            .FirstOrDefaultAsync(cancellationToken);
        return checkIn is null
            ? null
            : await BodyCheckInQuerySupport.MapAsync(checkIn, _mapper, _mediaResponseFactory, cancellationToken);
    }
}

public class GetBodyCheckInSummaryHandler : IRequestHandler<GetBodyCheckInSummaryQuery, BodyCheckInSummaryResponse>
{
    private readonly FitspireDbContext _context;
    private readonly IBodyCheckInTimeZoneService _timeZoneService;

    public GetBodyCheckInSummaryHandler(FitspireDbContext context, IBodyCheckInTimeZoneService timeZoneService)
    {
        _context = context;
        _timeZoneService = timeZoneService;
    }

    public async Task<BodyCheckInSummaryResponse> Handle(GetBodyCheckInSummaryQuery query, CancellationToken cancellationToken)
    {
        var today = await _timeZoneService.GetTodayAsync(query.UserId, cancellationToken);
        var to = query.Filter.To ?? today;
        var from = query.Filter.From ?? to.AddDays(-30);
        if (to > today || to < from || to.DayNumber - from.DayNumber > 366)
            throw new DomainException("Summary date range must be between zero and 366 days.");

        var entries = await _context.BodyCheckIns.AsNoTracking()
            .Where(checkIn => checkIn.UserId == query.UserId && checkIn.DeletedAt == null && checkIn.CheckInDate <= to)
            .OrderBy(checkIn => checkIn.CheckInDate).ThenBy(checkIn => checkIn.Id).ToListAsync(cancellationToken);
        var inRange = entries.Where(checkIn => checkIn.CheckInDate >= from).ToList();
        var baseline = BodyCheckInSummaryFactory.CreateBaseline(entries, inRange, from);
        var current = BodyCheckInSummaryFactory.CreateSnapshot(entries);
        var changes = BodyCheckInSummaryFactory.CreateChanges(baseline, current);
        var points = inRange.Select(BodyCheckInSummaryFactory.CreateChartPoint).ToList();
        var latestWellbeing = entries.Select(checkIn => checkIn.WellbeingScore).LastOrDefault(score => score.HasValue);

        return new BodyCheckInSummaryResponse(from, to, inRange.Count, baseline, current, changes, latestWellbeing, points);
    }
}

internal static class BodyCheckInQuerySupport
{
    public static IQueryable<BodyCheckIn> ActiveForUser(FitspireDbContext context, Guid userId) => context.BodyCheckIns.AsNoTracking()
        .Include(checkIn => checkIn.PhotoMedia).ThenInclude(media => media!.Variants)
        .Where(checkIn => checkIn.UserId == userId && checkIn.DeletedAt == null);

    public static async Task<BodyCheckInResponse> MapAsync(BodyCheckIn checkIn, IMapper mapper,
        IMediaResponseFactory mediaResponseFactory, CancellationToken cancellationToken)
    {
        var photo = await mediaResponseFactory.CreateAsync(checkIn.PhotoMedia, cancellationToken);
        return mapper.Map<BodyCheckInResponse>(checkIn) with { Photo = photo };
    }

    public static async Task<IReadOnlyList<BodyCheckInResponse>> MapManyAsync(IReadOnlyList<BodyCheckIn> checkIns,
        IMapper mapper, IMediaResponseFactory mediaResponseFactory, CancellationToken cancellationToken)
    {
        var photos = await mediaResponseFactory.CreateManyAsync(checkIns.Select(checkIn => checkIn.PhotoMedia).OfType<backend.Modules.Media.Domain.MediaAsset>(), cancellationToken);
        return checkIns.Select(checkIn => mapper.Map<BodyCheckInResponse>(checkIn) with
        {
            Photo = checkIn.PhotoMedia is null ? null : photos.GetValueOrDefault(checkIn.PhotoMedia.Id)
        }).ToList();
    }
}

internal static class BodyCheckInSummaryFactory
{
    public static BodyMeasurementSnapshotResponse CreateBaseline(IReadOnlyList<BodyCheckIn> entries,
        IReadOnlyList<BodyCheckIn> inRange, DateOnly from) => CreateSnapshot(entries.Where(checkIn => checkIn.CheckInDate <= from), inRange);

    public static BodyMeasurementSnapshotResponse CreateSnapshot(IEnumerable<BodyCheckIn> entries,
        IEnumerable<BodyCheckIn>? fallback = null)
    {
        var items = entries.ToList();
        var fallbacks = fallback?.ToList() ?? [];
        return new BodyMeasurementSnapshotResponse(
            LastOrFirst(items, fallbacks, checkIn => checkIn.WeightKg),
            LastOrFirst(items, fallbacks, checkIn => checkIn.BodyFatPercent),
            LastOrFirst(items, fallbacks, checkIn => checkIn.WaistCm),
            LastOrFirst(items, fallbacks, checkIn => checkIn.ChestCm),
            LastOrFirst(items, fallbacks, checkIn => checkIn.HipsCm),
            LastOrFirst(items, fallbacks, checkIn => checkIn.ArmCm),
            LastOrFirst(items, fallbacks, checkIn => checkIn.ThighCm));
    }

    public static BodyMeasurementChangeResponse CreateChanges(BodyMeasurementSnapshotResponse baseline,
        BodyMeasurementSnapshotResponse current) => new(
        Difference(baseline.WeightKg, current.WeightKg), Difference(baseline.BodyFatPercent, current.BodyFatPercent),
        Difference(baseline.WaistCm, current.WaistCm), Difference(baseline.ChestCm, current.ChestCm),
        Difference(baseline.HipsCm, current.HipsCm), Difference(baseline.ArmCm, current.ArmCm), Difference(baseline.ThighCm, current.ThighCm));

    public static BodyCheckInChartPoint CreateChartPoint(BodyCheckIn checkIn) => new(checkIn.CheckInDate,
        checkIn.WeightKg, checkIn.BodyFatPercent, checkIn.WaistCm, checkIn.ChestCm, checkIn.HipsCm,
        checkIn.ArmCm, checkIn.ThighCm, checkIn.WellbeingScore);

    private static double? LastOrFirst(IReadOnlyList<BodyCheckIn> items, IReadOnlyList<BodyCheckIn> fallbacks,
        Func<BodyCheckIn, double?> selector) => items.Select(selector).LastOrDefault(value => value.HasValue)
        ?? fallbacks.Select(selector).FirstOrDefault(value => value.HasValue);

    private static double? Difference(double? baseline, double? current) =>
        baseline.HasValue && current.HasValue ? current.Value - baseline.Value : null;
}
