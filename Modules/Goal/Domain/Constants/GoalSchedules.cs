namespace backend.Modules.Goal.Domain.Constants;

public static class GoalSchedules
{
    public const string OneOff = "one-off";
    public const string Daily = "daily";
    public const string Weekly = "weekly";
    public const string Monthly = "monthly";

    public static readonly IReadOnlySet<string> Recurring = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Daily,
        Weekly,
        Monthly
    };

    public static readonly IReadOnlySet<string> All = new HashSet<string>(Recurring, StringComparer.OrdinalIgnoreCase)
    {
        OneOff
    };
}
