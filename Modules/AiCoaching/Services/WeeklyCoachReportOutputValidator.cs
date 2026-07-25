using System.Text.Json;
using backend.Modules.AiCoaching.Contracts;

namespace backend.Modules.AiCoaching.Services;

public interface IWeeklyCoachReportOutputValidator
{
    string ValidateAndNormalize(string reportJson, string snapshotJson);
}

public sealed class WeeklyCoachReportOutputValidator : IWeeklyCoachReportOutputValidator
{
    private const int MaximumReportJsonLength = 20_000;
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private static readonly HashSet<string> Categories = new(StringComparer.Ordinal)
    {
        "Workout", "Consistency", "Recovery", "Nutrition", "Wellbeing", "Goal", "Challenge"
    };

    public string ValidateAndNormalize(string reportJson, string snapshotJson)
    {
        if (string.IsNullOrWhiteSpace(reportJson) || reportJson.Length > MaximumReportJsonLength)
            throw Invalid("The AI report exceeds the allowed size.");

        var report = Deserialize<WeeklyCoachStructuredReport>(reportJson, "report");
        var snapshot = Deserialize<WeeklyCoachSnapshot>(snapshotJson, "snapshot");
        var evidenceKeys = snapshot.Evidence.Select(item => item.Key).ToHashSet(StringComparer.Ordinal);

        ValidateText(report.Headline, 120, "headline");
        ValidateText(report.Overview, 900, "overview");
        ValidateObservations(report.Wins, evidenceKeys, "wins");
        ValidateObservations(report.Patterns, evidenceKeys, "patterns");
        ValidateActions(report.NextWeekActions, evidenceKeys);
        ValidateLimitations(report.DataLimitations);

        return JsonSerializer.Serialize(report, SerializerOptions);
    }

    private static T Deserialize<T>(string json, string valueName)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(json, SerializerOptions)
                   ?? throw Invalid($"The AI {valueName} is empty.");
        }
        catch (JsonException exception)
        {
            throw Invalid($"The AI {valueName} is not valid JSON.", exception);
        }
    }

    private static void ValidateObservations(IReadOnlyList<WeeklyCoachObservation>? items,
        IReadOnlySet<string> evidenceKeys, string name)
    {
        if (items is null || items.Count > 3)
            throw Invalid($"The AI {name} collection is invalid.");

        foreach (var item in items)
        {
            ValidateText(item.Title, 100, $"{name} title");
            ValidateText(item.Explanation, 360, $"{name} explanation");
            ValidateCategory(item.Category, name);
            ValidateEvidenceKeys(item.EvidenceKeys, evidenceKeys, name, required: true);
        }
    }

    private static void ValidateActions(IReadOnlyList<WeeklyCoachAction>? actions, IReadOnlySet<string> evidenceKeys)
    {
        if (actions is null || actions.Count is < 1 or > 3)
            throw Invalid("The AI next-week actions collection is invalid.");

        foreach (var action in actions)
        {
            ValidateText(action.Title, 100, "action title");
            ValidateText(action.Explanation, 360, "action explanation");
            ValidateCategory(action.Category, "action");
            ValidateEvidenceKeys(action.EvidenceKeys, evidenceKeys, "action", required: false);
        }
    }

    private static void ValidateLimitations(IReadOnlyList<string>? limitations)
    {
        if (limitations is null || limitations.Count > 3)
            throw Invalid("The AI data-limitations collection is invalid.");
        foreach (var limitation in limitations)
            ValidateText(limitation, 240, "data limitation");
    }

    private static void ValidateEvidenceKeys(IReadOnlyList<string>? keys, IReadOnlySet<string> evidenceKeys,
        string itemName, bool required)
    {
        if (keys is null || keys.Count > 4 || (required && keys.Count == 0) || keys.Distinct(StringComparer.Ordinal).Count() != keys.Count ||
            keys.Any(key => string.IsNullOrWhiteSpace(key) || key.Length > 100 || !evidenceKeys.Contains(key)))
        {
            throw Invalid($"The AI {itemName} contains invalid evidence references.");
        }
    }

    private static void ValidateCategory(string? category, string itemName)
    {
        if (string.IsNullOrWhiteSpace(category) || !Categories.Contains(category))
            throw Invalid($"The AI {itemName} category is invalid.");
    }

    private static void ValidateText(string? value, int maximumLength, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > maximumLength || value.Contains('<') || value.Contains('>') ||
            value.Contains("http://", StringComparison.OrdinalIgnoreCase) || value.Contains("https://", StringComparison.OrdinalIgnoreCase))
        {
            throw Invalid($"The AI {fieldName} is invalid.");
        }
    }

    private static AiProviderException Invalid(string message, Exception? innerException = null) =>
        new(AiProviderFailureKind.InvalidResponse, message, false, innerException);
}

public sealed record WeeklyCoachStructuredReport(
    string Headline,
    string Overview,
    IReadOnlyList<WeeklyCoachObservation> Wins,
    IReadOnlyList<WeeklyCoachObservation> Patterns,
    IReadOnlyList<WeeklyCoachAction> NextWeekActions,
    IReadOnlyList<string> DataLimitations);

public sealed record WeeklyCoachObservation(string Title, string Explanation, string Category, IReadOnlyList<string> EvidenceKeys);

public sealed record WeeklyCoachAction(string Title, string Explanation, string Category, IReadOnlyList<string> EvidenceKeys);
