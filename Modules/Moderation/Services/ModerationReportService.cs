using AutoMapper;
using backend.Data;
using backend.Modules.Moderation.Contracts;
using backend.Modules.Moderation.Domain;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Data;

namespace backend.Modules.Moderation.Services;

public sealed class ModerationReportService : IModerationReportService
{
    private const string SnapshotVersion = "v1";

    private readonly FitspireDbContext _context;
    private readonly IModerationTargetResolver _targetResolver;
    private readonly IMapper _mapper;

    public ModerationReportService(FitspireDbContext context, IModerationTargetResolver targetResolver, IMapper mapper)
    {
        _context = context;
        _targetResolver = targetResolver;
        _mapper = mapper;
    }

    public async Task<ModerationReportSubmissionResponse> CreateAsync(
        Guid reporterUserId,
        CreateModerationReportRequest request,
        CancellationToken cancellationToken = default)
    {
        var target = await _targetResolver.ResolveAsync(reporterUserId, request.TargetType, request.TargetId, cancellationToken);
        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        var existing = await FindOpenReportAsync(reporterUserId, target.TargetType, target.TargetId, cancellationToken);
        if (existing is not null)
        {
            await transaction.CommitAsync(cancellationToken);
            return Map(existing, true);
        }

        var report = ModerationReport.Create(Guid.NewGuid(), reporterUserId, target.SubjectUserId, target.TargetType, target.TargetId,
            target.MediaContext, request.Reason, request.Details, target.SnapshotJson, SnapshotVersion, DateTime.UtcNow);
        _context.ModerationReports.Add(report);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Map(report, false);
        }
        catch (DbUpdateException exception) when (IsOpenReportConflict(exception))
        {
            await transaction.RollbackAsync(cancellationToken);
            _context.Entry(report).State = EntityState.Detached;
            var concurrent = await FindOpenReportAsync(reporterUserId, target.TargetType, target.TargetId, cancellationToken);
            if (concurrent is null)
                throw;

            return Map(concurrent, true);
        }
    }

    private Task<ModerationReport?> FindOpenReportAsync(Guid reporterUserId, ModerationReportTargetType targetType, Guid targetId,
        CancellationToken cancellationToken) => _context.ModerationReports.AsNoTracking().FirstOrDefaultAsync(report =>
        report.ReporterUserId == reporterUserId && report.TargetType == targetType && report.TargetId == targetId &&
        report.Status == ModerationReportStatus.Open, cancellationToken);

    private ModerationReportSubmissionResponse Map(ModerationReport report, bool alreadyReported) =>
        _mapper.Map<ModerationReportSubmissionResponse>(report) with { AlreadyReported = alreadyReported };

    private static bool IsOpenReportConflict(DbUpdateException exception) =>
        exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };
}
