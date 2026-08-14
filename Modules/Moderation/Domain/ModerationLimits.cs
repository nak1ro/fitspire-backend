namespace backend.Modules.Moderation.Domain;

public static class ModerationLimits
{
    public const int MaximumReportDetailsLength = 1_000;
    public const int MaximumResolutionNoteLength = 1_000;
    public const int MaximumActionNoteLength = 1_000;
    public const int MaximumSuspensionReasonLength = 500;
    public const int MaximumTargetSnapshotLength = 16_000;
    public const int MaximumSuspensionDurationDays = 365;
}
