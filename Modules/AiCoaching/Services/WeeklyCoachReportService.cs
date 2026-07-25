using System.Data;
using AutoMapper;
using backend.Data;
using backend.Modules.AiCoaching.Configuration;
using backend.Modules.AiCoaching.Contracts;
using backend.Modules.AiCoaching.Domain;
using backend.Modules.Shared.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;
using Npgsql;

namespace backend.Modules.AiCoaching.Services;

public sealed class WeeklyCoachReportService : IWeeklyCoachReportService
{
    private readonly FitspireDbContext _context;
    private readonly IWeeklyCoachPeriodService _periodService;
    private readonly IWeeklyCoachSnapshotBuilder _snapshotBuilder;
    private readonly IWeeklyCoachReportResponseFactory _responseFactory;
    private readonly IMapper _mapper;
    private readonly OpenAiOptions _options;

    public WeeklyCoachReportService(FitspireDbContext context, IWeeklyCoachPeriodService periodService,
        IWeeklyCoachSnapshotBuilder snapshotBuilder, IWeeklyCoachReportResponseFactory responseFactory, IMapper mapper,
        IOptions<OpenAiOptions> options)
    {
        _context = context;
        _periodService = periodService;
        _snapshotBuilder = snapshotBuilder;
        _responseFactory = responseFactory;
        _mapper = mapper;
        _options = options.Value;
    }

    public async Task<WeeklyCoachGenerationResponse> RequestGenerationAsync(Guid userId, GenerateWeeklyCoachReportRequest request,
        CancellationToken cancellationToken)
    {
        EnsureAiAvailable();
        var now = DateTime.UtcNow;
        var period = await _periodService.ResolveCompletedAsync(userId, request.PeriodStart, now, cancellationToken);
        var snapshot = await _snapshotBuilder.BuildAsync(userId, period, cancellationToken);
        var source = new WeeklyCoachReportSource(snapshot.SourceFingerprint, WeeklyCoachSnapshotVersions.Snapshot,
            snapshot.SnapshotJson, WeeklyCoachPromptCatalogue.Version, WeeklyCoachStructuredOutputSchema.Version);
        return await QueueWithRetryAsync(userId, period, source, now, cancellationToken);
    }

    public async Task<WeeklyCoachReportResponse> GetAsync(Guid userId, Guid reportId, CancellationToken cancellationToken)
    {
        var report = await OwnerReports(userId).FirstOrDefaultAsync(candidate => candidate.Id == reportId, cancellationToken)
            ?? throw new NotFoundException("Coaching report was not found.");
        return _responseFactory.Create(report);
    }

    public async Task<WeeklyCoachReportPageResponse> GetHistoryAsync(Guid userId, WeeklyCoachReportHistoryFilter filter,
        CancellationToken cancellationToken)
    {
        var query = OwnerReports(userId).OrderByDescending(report => report.PeriodStart);
        var total = await query.CountAsync(cancellationToken);
        var reports = await query.Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync(cancellationToken);
        return new WeeklyCoachReportPageResponse(_mapper.Map<IReadOnlyList<WeeklyCoachReportListItemResponse>>(reports),
            filter.Page, filter.PageSize, total);
    }

    public async Task DeleteAsync(Guid userId, Guid reportId, CancellationToken cancellationToken)
    {
        var report = await _context.WeeklyCoachReports.FirstOrDefaultAsync(candidate => candidate.Id == reportId &&
            candidate.UserId == userId, cancellationToken) ?? throw new NotFoundException("Coaching report was not found.");
        _context.WeeklyCoachReports.Remove(report);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<WeeklyCoachGenerationResponse> QueueAsync(Guid userId, WeeklyCoachPeriod period,
        WeeklyCoachReportSource source, DateTime utcNow, CancellationToken cancellationToken)
    {
        var ownsTransaction = _context.Database.CurrentTransaction is null;
        await using var transaction = ownsTransaction
            ? await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken)
            : null;
        try
        {
            var existingReport = await _context.WeeklyCoachReports.FirstOrDefaultAsync(candidate => candidate.UserId == userId &&
                candidate.PeriodStart == period.PeriodStart, cancellationToken);
            var decision = ApplyQueueDecision(existingReport, userId, period, source, utcNow);

            await _context.SaveChangesAsync(cancellationToken);
            if (transaction is not null)
                await transaction.CommitAsync(cancellationToken);
            return new WeeklyCoachGenerationResponse(_responseFactory.Create(decision.Report), decision.Accepted);
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception) && ownsTransaction)
        {
            if (transaction is not null)
                await transaction.RollbackAsync(cancellationToken);
            _context.ChangeTracker.Clear();
            return await ReadConcurrentReportAsync(userId, period.PeriodStart, cancellationToken);
        }
    }

    private async Task<WeeklyCoachGenerationResponse> QueueWithRetryAsync(Guid userId, WeeklyCoachPeriod period,
        WeeklyCoachReportSource source, DateTime utcNow, CancellationToken cancellationToken)
    {
        const int maximumAttempts = 2;
        for (var attempt = 1; attempt <= maximumAttempts; attempt++)
        {
            try
            {
                return await QueueAsync(userId, period, source, utcNow, cancellationToken);
            }
            catch (DbUpdateException exception) when (IsSerializationFailure(exception) && attempt < maximumAttempts)
            {
                _context.ChangeTracker.Clear();
            }
            catch (DbUpdateException exception) when (IsSerializationFailure(exception))
            {
                throw new ConflictException("Coaching report generation conflicted. Please try again.");
            }
        }

        throw new ConflictException("Coaching report generation conflicted. Please try again.");
    }

    private QueueDecision ApplyQueueDecision(WeeklyCoachReport? report, Guid userId, WeeklyCoachPeriod period,
        WeeklyCoachReportSource source, DateTime utcNow)
    {
        if (report is null)
        {
            report = WeeklyCoachReport.CreatePending(Guid.NewGuid(), userId, period.PeriodStart, period.TimeZoneId, source, utcNow);
            _context.WeeklyCoachReports.Add(report);
            return new QueueDecision(report, true);
        }

        if (report.MatchesCompletedSource(source.SourceFingerprint))
            return new QueueDecision(report, false);
        if (report.Status is WeeklyCoachReportStatus.Pending or WeeklyCoachReportStatus.Processing)
            return new QueueDecision(report, true);

        report.QueueReplacement(source, utcNow);
        return new QueueDecision(report, true);
    }

    private async Task<WeeklyCoachGenerationResponse> ReadConcurrentReportAsync(Guid userId, DateOnly periodStart,
        CancellationToken cancellationToken)
    {
        var report = await OwnerReports(userId).FirstOrDefaultAsync(candidate => candidate.PeriodStart == periodStart, cancellationToken)
            ?? throw new ConflictException("Coaching report generation conflicted. Please try again.");
        return new WeeklyCoachGenerationResponse(_responseFactory.Create(report),
            report.Status is WeeklyCoachReportStatus.Pending or WeeklyCoachReportStatus.Processing);
    }

    private IQueryable<WeeklyCoachReport> OwnerReports(Guid userId) => _context.WeeklyCoachReports.AsNoTracking()
        .Where(report => report.UserId == userId);

    private void EnsureAiAvailable()
    {
        if (!_options.Enabled || string.IsNullOrWhiteSpace(_options.ApiKey))
            throw new AiServiceUnavailableException("AI coaching is not configured.");
    }

    private static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };

    private static bool IsSerializationFailure(DbUpdateException exception) =>
        exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.SerializationFailure };

    private sealed record QueueDecision(WeeklyCoachReport Report, bool Accepted);
}
