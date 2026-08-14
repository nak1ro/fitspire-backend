namespace backend.Modules.AiCoaching.Domain;

public static class AiCoachInteractionLimits
{
    public const int MaximumQuestionLength = 2_000;
    public const int MaximumThreadTitleLength = 100;
    public const int MaximumAnswerMarkdownLength = 2_200;
    public const int MaximumDailySummaryMarkdownLength = 1_200;
    public const int MaximumDailyInsightMarkdownLength = 700;
    public const int MaximumActionTitleLength = 100;
    public const int MaximumActionDescriptionLength = 360;
    public const int MaximumThreadSummaryLength = 1_400;
    public const int MaximumLimitationLength = 240;
    public const int MaximumEvidenceKeys = 6;
    public const int MaximumSuggestedActions = 3;
    public const int MaximumDataLimitations = 3;
}
