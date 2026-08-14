using backend.Modules.Shared.Domain;

namespace backend.Modules.AiCoaching.Domain;

internal static class AiCoachDomainRules
{
    public static void EnsureUtc(DateTime value, string name)
    {
        if (value.Kind != DateTimeKind.Utc)
            throw new DomainException($"{name} must be in UTC.");
    }

    public static string NormalizeRequired(string? value, int maximumLength, string name)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized) || normalized.Length > maximumLength)
            throw new DomainException($"{name} is required and must be at most {maximumLength} characters.");

        return normalized;
    }

    public static void EnsureNonEmpty(Guid value, string name)
    {
        if (value == Guid.Empty)
            throw new DomainException($"{name} is required.");
    }

    public static void EnsureCompletionUsage(CoachGenerationCompletion completion)
    {
        if (completion.InputTokens < 0 || completion.OutputTokens < 0 || completion.TotalTokens < 0)
            throw new DomainException("AI token usage cannot be negative.");
    }
}
