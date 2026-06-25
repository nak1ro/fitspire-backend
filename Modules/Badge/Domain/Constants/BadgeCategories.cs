namespace backend.Modules.Badge.Domain.Constants;

public static class BadgeCategories
{
    public const string Workout = "Workout";
    public const string Consistency = "Consistency";
    public const string Distance = "Distance";
    public const string Duration = "Duration";
    public const string Strength = "Strength";
    public const string PersonalRecord = "PersonalRecord";
    public const string Goal = "Goal";
    public const string Challenge = "Challenge";
    public const string Social = "Social";

    private static readonly HashSet<string> Known =
    [Workout, Consistency, Distance, Duration, Strength, PersonalRecord, Goal, Challenge, Social];

    public static bool IsKnown(string? value) => value is not null && Known.Contains(value);
}
