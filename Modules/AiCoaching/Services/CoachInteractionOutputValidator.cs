using System.Text.Json;
using backend.Modules.AiCoaching.Contracts;
using backend.Modules.AiCoaching.Domain;

namespace backend.Modules.AiCoaching.Services;

public interface ICoachInteractionOutputValidator
{
    string ValidateAndNormalizeAnswer(string outputJson, IReadOnlySet<string> evidenceKeys);
    string ValidateAndNormalizeDailyBriefing(string outputJson, IReadOnlySet<string> evidenceKeys);
}

public sealed class CoachInteractionOutputValidator : ICoachInteractionOutputValidator
{
    private const int MaximumOutputJsonLength = 20_000;
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private static readonly HashSet<string> ActionCategories = new(StringComparer.Ordinal)
    {
        "Workout", "Recovery", "Consistency", "Nutrition", "Wellbeing", "Goal", "Challenge", "GeneralFitness"
    };
    private static readonly HashSet<string> SafetyCategories = new(StringComparer.Ordinal)
    {
        "None", "GeneralCaution", "MedicalBoundary"
    };
    private static readonly HashSet<string> DailyFocuses = new(StringComparer.Ordinal)
    {
        "Train", "Recover", "StayConsistent", "Plan", "Nutrition", "Wellbeing", "InsufficientData"
    };

    public string ValidateAndNormalizeAnswer(string outputJson, IReadOnlySet<string> evidenceKeys)
    {
        var answer = Deserialize<CoachAnswerStructuredOutput>(outputJson, "answer");

        MarkdownContentValidator.ValidateRequired(answer.AnswerMarkdown, AiCoachInteractionLimits.MaximumAnswerMarkdownLength,
            "answer Markdown");
        ValidateActions(answer.SuggestedActions, evidenceKeys);
        ValidateLimitations(answer.DataLimitations);
        ValidateEvidenceKeys(answer.EvidenceKeys, evidenceKeys, "answer evidence");
        MarkdownContentValidator.ValidatePlainText(answer.UpdatedThreadSummary,
            AiCoachInteractionLimits.MaximumThreadSummaryLength, "thread summary");
        ValidateEnum(answer.SafetyCategory, SafetyCategories, "safety category");

        return JsonSerializer.Serialize(answer, SerializerOptions);
    }

    public string ValidateAndNormalizeDailyBriefing(string outputJson, IReadOnlySet<string> evidenceKeys)
    {
        var briefing = Deserialize<DailyCoachBriefingStructuredOutput>(outputJson, "daily briefing");

        MarkdownContentValidator.ValidatePlainText(briefing.Headline, 120, "daily headline");
        ValidateEnum(briefing.Focus, DailyFocuses, "daily focus");
        MarkdownContentValidator.ValidateRequired(briefing.SummaryMarkdown,
            AiCoachInteractionLimits.MaximumDailySummaryMarkdownLength, "daily summary Markdown");
        ValidateAction(briefing.NextAction, evidenceKeys, "daily next action");
        MarkdownContentValidator.ValidateRequired(briefing.InsightMarkdown,
            AiCoachInteractionLimits.MaximumDailyInsightMarkdownLength, "daily insight Markdown");
        ValidateLimitations(briefing.DataLimitations);
        ValidateEvidenceKeys(briefing.EvidenceKeys, evidenceKeys, "daily evidence");

        return JsonSerializer.Serialize(briefing, SerializerOptions);
    }

    private static T Deserialize<T>(string outputJson, string valueName)
    {
        if (string.IsNullOrWhiteSpace(outputJson) || outputJson.Length > MaximumOutputJsonLength)
            throw Invalid($"The AI {valueName} exceeds the allowed size.");

        try
        {
            return JsonSerializer.Deserialize<T>(outputJson, SerializerOptions)
                   ?? throw Invalid($"The AI {valueName} is empty.");
        }
        catch (JsonException exception)
        {
            throw Invalid($"The AI {valueName} is not valid JSON.", exception);
        }
    }

    private static void ValidateActions(IReadOnlyList<CoachSuggestedAction>? actions, IReadOnlySet<string> evidenceKeys)
    {
        if (actions is null || actions.Count > AiCoachInteractionLimits.MaximumSuggestedActions)
            throw Invalid("The AI suggested-actions collection is invalid.");

        foreach (var action in actions)
            ValidateAction(action, evidenceKeys, "suggested action");
    }

    private static void ValidateAction(CoachSuggestedAction? action, IReadOnlySet<string> evidenceKeys, string fieldName)
    {
        if (action is null)
            throw Invalid($"The AI {fieldName} is invalid.");

        MarkdownContentValidator.ValidatePlainText(action.Title, AiCoachInteractionLimits.MaximumActionTitleLength,
            $"{fieldName} title");
        MarkdownContentValidator.ValidateRequired(action.Description, AiCoachInteractionLimits.MaximumActionDescriptionLength,
            $"{fieldName} description");
        ValidateEnum(action.Category, ActionCategories, $"{fieldName} category");
        ValidateEvidenceKeys(action.EvidenceKeys, evidenceKeys, $"{fieldName} evidence");
    }

    private static void ValidateLimitations(IReadOnlyList<string>? limitations)
    {
        if (limitations is null || limitations.Count > AiCoachInteractionLimits.MaximumDataLimitations)
            throw Invalid("The AI data-limitations collection is invalid.");

        foreach (var limitation in limitations)
            MarkdownContentValidator.ValidatePlainText(limitation, AiCoachInteractionLimits.MaximumLimitationLength,
                "data limitation");
    }

    private static void ValidateEvidenceKeys(IReadOnlyList<string>? keys, IReadOnlySet<string> availableKeys, string fieldName)
    {
        if (keys is null || keys.Count > AiCoachInteractionLimits.MaximumEvidenceKeys ||
            keys.Distinct(StringComparer.Ordinal).Count() != keys.Count ||
            keys.Any(key => string.IsNullOrWhiteSpace(key) || key.Length > 100 || !availableKeys.Contains(key)))
        {
            throw Invalid($"The AI {fieldName} contains invalid references.");
        }
    }

    private static void ValidateEnum(string? value, IReadOnlySet<string> permittedValues, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value) || !permittedValues.Contains(value))
            throw Invalid($"The AI {fieldName} is invalid.");
    }

    private static AiProviderException Invalid(string message, Exception? innerException = null) =>
        new(AiProviderFailureKind.InvalidResponse, message, false, innerException);
}

public sealed record CoachAnswerStructuredOutput(
    string AnswerMarkdown,
    IReadOnlyList<CoachSuggestedAction> SuggestedActions,
    IReadOnlyList<string> DataLimitations,
    IReadOnlyList<string> EvidenceKeys,
    string UpdatedThreadSummary,
    string SafetyCategory);

public sealed record DailyCoachBriefingStructuredOutput(
    string Headline,
    string Focus,
    string SummaryMarkdown,
    CoachSuggestedAction NextAction,
    string InsightMarkdown,
    IReadOnlyList<string> DataLimitations,
    IReadOnlyList<string> EvidenceKeys);

public sealed record CoachSuggestedAction(
    string Title,
    string Description,
    string Category,
    IReadOnlyList<string> EvidenceKeys);
