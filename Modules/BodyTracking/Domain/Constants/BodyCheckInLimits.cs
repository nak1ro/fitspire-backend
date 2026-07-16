namespace backend.Modules.BodyTracking.Domain.Constants;

public static class BodyCheckInLimits
{
    public const double MaximumWeightKg = 1_000;
    public const double MaximumBodyFatPercent = 100;
    public const double MaximumCircumferenceCm = 500;
    public const int MinimumWellbeingScore = 1;
    public const int MaximumWellbeingScore = 5;
    public const int MaximumNoteLength = 1_000;
}
