using backend.Modules.Shared.Domain;
using backend.Modules.User.Domain;

namespace backend.Modules.Moderation.Domain;

public sealed class ModerationAction : Entity<Guid>
{
    public Guid ReportId { get; private set; }
    public Guid ModeratorUserId { get; private set; }
    public Guid SubjectUserId { get; private set; }
    public ModerationReportTargetType TargetType { get; private set; }
    public Guid TargetId { get; private set; }
    public ModerationActionType ActionType { get; private set; }
    public string? Note { get; private set; }
    public DateTime? SuspensionEndsAtUtc { get; private set; }
    public DateTime OccurredAtUtc { get; private set; }

    public ModerationReport Report { get; private set; } = null!;
    public AppUser ModeratorUser { get; private set; } = null!;
    public AppUser SubjectUser { get; private set; } = null!;

    private ModerationAction()
    {
    }

    public static ModerationAction Create(
        Guid reportId,
        Guid moderatorUserId,
        Guid subjectUserId,
        ModerationReportTargetType targetType,
        Guid targetId,
        ModerationActionType actionType,
        string? note,
        DateTime? suspensionEndsAtUtc,
        DateTime utcNow)
    {
        ModerationDomainRules.EnsureNonEmpty(reportId, "Report identity");
        ModerationDomainRules.EnsureNonEmpty(moderatorUserId, "Moderator");
        ModerationDomainRules.EnsureNonEmpty(subjectUserId, "Reported account");
        ModerationDomainRules.EnsureNonEmpty(targetId, "Report target");
        ModerationDomainRules.EnsureUtc(utcNow, nameof(utcNow));

        if (suspensionEndsAtUtc is not null)
        {
            ModerationDomainRules.EnsureUtc(suspensionEndsAtUtc.Value, nameof(suspensionEndsAtUtc));
            if (suspensionEndsAtUtc <= utcNow)
                throw new DomainException("Suspension end time must be in the future.");
        }

        if (actionType == ModerationActionType.UserSuspended && suspensionEndsAtUtc is null)
            throw new DomainException("A user suspension action requires an end time.");

        if (actionType != ModerationActionType.UserSuspended && suspensionEndsAtUtc is not null)
            throw new DomainException("Only user suspension actions can include an end time.");

        return new ModerationAction
        {
            Id = Guid.NewGuid(),
            ReportId = reportId,
            ModeratorUserId = moderatorUserId,
            SubjectUserId = subjectUserId,
            TargetType = targetType,
            TargetId = targetId,
            ActionType = actionType,
            Note = ModerationDomainRules.NormalizeOptional(note, ModerationLimits.MaximumActionNoteLength, "Action note"),
            SuspensionEndsAtUtc = suspensionEndsAtUtc,
            OccurredAtUtc = utcNow,
            CreatedAt = utcNow
        };
    }
}
