namespace backend.Modules.Badge.Domain.Constants;

public static class BadgeTiers
{
    public const string None = "None";
    public const string Bronze = "Bronze";
    public const string Silver = "Silver";
    public const string Gold = "Gold";

    private static readonly HashSet<string> Known = [None, Bronze, Silver, Gold];

    public static bool IsKnown(string? value) => value is not null && Known.Contains(value);
}
