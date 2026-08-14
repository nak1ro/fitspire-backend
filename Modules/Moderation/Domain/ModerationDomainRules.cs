using backend.Modules.Shared.Domain;

namespace backend.Modules.Moderation.Domain;

internal static class ModerationDomainRules
{
    public static void EnsureNonEmpty(Guid value, string name)
    {
        if (value == Guid.Empty)
            throw new DomainException($"{name} is required.");
    }

    public static void EnsureUtc(DateTime value, string name)
    {
        if (value.Kind != DateTimeKind.Utc)
            throw new DomainException($"{name} must be in UTC.");
    }

    public static string? NormalizeOptional(string? value, int maximumLength, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var normalized = value.Trim();
        if (normalized.Length > maximumLength)
            throw new DomainException($"{name} must be at most {maximumLength} characters.");

        return normalized;
    }

    public static string NormalizeRequired(string? value, int maximumLength, string name)
    {
        var normalized = NormalizeOptional(value, maximumLength, name);
        return normalized ?? throw new DomainException($"{name} is required.");
    }
}
