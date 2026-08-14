using System.Text.RegularExpressions;
using backend.Modules.AiCoaching.Contracts;

namespace backend.Modules.AiCoaching.Services;

public static partial class MarkdownContentValidator
{
    [GeneratedRegex(@"(?m)^#{4,}\\s")]
    private static partial Regex DeepHeadingRegex();

    [GeneratedRegex(@"\\[[^]]*\\]\\([^)]*\\)")]
    private static partial Regex LinkRegex();

    public static void ValidateRequired(string? value, int maximumLength, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > maximumLength)
            throw Invalid($"The AI {fieldName} is invalid.");

        ValidateSafeSyntax(value, fieldName);
    }

    public static void ValidatePlainText(string? value, int maximumLength, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > maximumLength || value.Contains('\n'))
            throw Invalid($"The AI {fieldName} is invalid.");

        ValidateSafeSyntax(value, fieldName);
    }

    private static void ValidateSafeSyntax(string value, string fieldName)
    {
        if (value.Contains("```", StringComparison.Ordinal) || value.Contains('<') || value.Contains('>') ||
            value.Contains("![", StringComparison.Ordinal) || LinkRegex().IsMatch(value) ||
            value.Contains("http://", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("https://", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("www.", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("javascript:", StringComparison.OrdinalIgnoreCase) || DeepHeadingRegex().IsMatch(value))
        {
            throw Invalid($"The AI {fieldName} contains unsupported Markdown.");
        }
    }

    private static AiProviderException Invalid(string message) =>
        new(AiProviderFailureKind.InvalidResponse, message, false);
}
