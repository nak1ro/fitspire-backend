namespace backend.Modules.AiCoaching.Configuration;

public sealed class AiCoachInteractionOptions
{
    public const string SectionName = "AiCoachInteraction";

    public int DailyQuestionLimit { get; init; } = 10;
    public int ConversationContextMessageLimit { get; init; } = 12;
    public int ConversationSnapshotLookbackDays { get; init; } = 28;
    public int NutritionSnapshotLookbackDays { get; init; } = 28;
    public int DailySnapshotLookbackDays { get; init; } = 7;
    public int WorkerPollSeconds { get; init; } = 10;
    public int ProcessingLeaseSeconds { get; init; } = 180;
}
