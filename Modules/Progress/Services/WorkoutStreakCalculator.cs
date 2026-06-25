namespace backend.Modules.Progress.Services;

public static class WorkoutStreakCalculator
{
    public static double GetLongestStreakDays(IEnumerable<DateTime> occurredAt, string timeZoneId)
    {
        var zone = ResolveTimeZone(timeZoneId);
        var dates = occurredAt.Select(ToUtc).Select(value => TimeZoneInfo.ConvertTimeFromUtc(value, zone).Date)
            .Distinct().OrderBy(value => value).ToList();
        var longest = 0;
        var current = 0;
        DateTime? previous = null;
        foreach (var date in dates)
        {
            current = previous?.AddDays(1) == date ? current + 1 : 1;
            longest = Math.Max(longest, current);
            previous = date;
        }

        return longest;
    }

    private static DateTime ToUtc(DateTime value) =>
        value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value, DateTimeKind.Utc);

    private static TimeZoneInfo ResolveTimeZone(string timeZoneId)
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId); }
        catch (TimeZoneNotFoundException) { return TimeZoneInfo.Utc; }
        catch (InvalidTimeZoneException) { return TimeZoneInfo.Utc; }
    }
}
