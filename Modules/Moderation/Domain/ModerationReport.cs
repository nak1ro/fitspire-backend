using backend.Modules.Shared.Domain;
using backend.Modules.User.Domain;

namespace backend.Modules.Moderation.Domain;

public sealed class ModerationReport : AggregateRoot<Guid>
{
    public Guid ReporterUserId { get; private set; }
    public Guid SubjectUserId { get; private set; }
    public ModerationReportTargetType TargetType { get; private set; }
    public Guid TargetId { get; private set; }
    public ModerationMediaContext? MediaContext { get; private set; }
    public ModerationReportReason Reason { get; private set; }
    public string? Details { get; private set; }
    public ModerationReportStatus Status { get; private set; }
    public ModerationResolutionOutcome? ResolutionOutcome { get; private set; }
    public Guid? ResolvedByUserId { get; private set; }
    public DateTime? ResolvedAtUtc { get; private set; }
    public string? ResolutionNote { get; private set; }
    public string TargetSnapshotJson { get; private set; } = null!;
    public string SnapshotVersion { get; private set; } = null!;
    public Guid ConcurrencyToken { get; private set; }

    public AppUser ReporterUser { get; private set; } = null!;
    public AppUser SubjectUser { get; private set; } = null!;
    public AppUser? ResolvedByUser { get; private set; }
    public ICollection<ModerationAction> Actions { get; private set; } = new List<ModerationAction>();

    private ModerationReport()
    {
    }

    public static ModerationReport Create(
        Guid id,
        Guid reporterUserId,
        Guid subjectUserId,
        ModerationReportTargetType targetType,
        Guid targetId,
        ModerationMediaContext? mediaContext,
        ModerationReportReason reason,
        string? details,
        string targetSnapshotJson,
        string snapshotVersion,
        DateTime utcNow)
    {
        ModerationDomainRules.EnsureNonEmpty(id, "Report identity");
        ModerationDomainRules.EnsureNonEmpty(reporterUserId, "Reporter");
        ModerationDomainRules.EnsureNonEmpty(subjectUserId, "Reported account");
        ModerationDomainRules.EnsureNonEmpty(targetId, "Report target");
        ModerationDomainRules.EnsureUtc(utcNow, nameof(utcNow));

        if (reporterUserId == subjectUserId)
            throw new DomainException("Users cannot report themselves or their own content.");

        if (!Enum.IsDefined(targetType) || !Enum.IsDefined(reason))
            throw new DomainException("The moderation report target or reason is invalid.");

        ValidateTarget(targetType, mediaContext);

        return new ModerationReport
        {
            Id = id,
            ReporterUserId = reporterUserId,
            SubjectUserId = subjectUserId,
            TargetType = targetType,
            TargetId = targetId,
            MediaContext = mediaContext,
            Reason = reason,
            Details = ModerationDomainRules.NormalizeOptional(details, ModerationLimits.MaximumReportDetailsLength,
                "Report details"),
            Status = ModerationReportStatus.Open,
            TargetSnapshotJson = ModerationDomainRules.NormalizeRequired(targetSnapshotJson,
                ModerationLimits.MaximumTargetSnapshotLength, "Target snapshot"),
            SnapshotVersion = ModerationDomainRules.NormalizeRequired(snapshotVersion, 60, "Snapshot version"),
            CreatedAt = utcNow,
            ConcurrencyToken = Guid.NewGuid()
        };
    }

    public void Resolve(ModerationResolutionOutcome outcome, Guid moderatorUserId, string? note, DateTime utcNow)
    {
        ModerationDomainRules.EnsureNonEmpty(moderatorUserId, "Moderator");
        ModerationDomainRules.EnsureUtc(utcNow, nameof(utcNow));
        if (Status != ModerationReportStatus.Open)
            throw new ConflictException("This moderation report has already been resolved.");

        Status = ModerationReportStatus.Resolved;
        ResolutionOutcome = outcome;
        ResolvedByUserId = moderatorUserId;
        ResolvedAtUtc = utcNow;
        ResolutionNote = ModerationDomainRules.NormalizeOptional(note, ModerationLimits.MaximumResolutionNoteLength,
            "Resolution note");
        Touch(utcNow);
    }

    private static void ValidateTarget(ModerationReportTargetType targetType, ModerationMediaContext? mediaContext)
    {
        if (mediaContext is not null && !Enum.IsDefined(mediaContext.Value))
            throw new DomainException("The moderation media context is invalid.");

        if (targetType == ModerationReportTargetType.Media && mediaContext is null)
            throw new DomainException("Reportable media must include its supported context.");

        if (targetType != ModerationReportTargetType.Media && mediaContext is not null)
            throw new DomainException("Only media reports can include media context.");
    }

    private void Touch(DateTime utcNow)
    {
        UpdatedAt = utcNow;
        ConcurrencyToken = Guid.NewGuid();
    }
}
