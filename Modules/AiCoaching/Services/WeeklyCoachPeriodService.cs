using backend.Data;
using backend.Modules.Shared.Domain;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.AiCoaching.Services;

public interface IWeeklyCoachPeriodService
{
    Task<WeeklyCoachPeriod> GetLatestCompletedAsync(Guid userId, DateTime utcNow, CancellationToken cancellationToken);
    Task<WeeklyCoachPeriod> ResolveCompletedAsync(Guid userId, DateOnly? periodStart, DateTime utcNow,
        CancellationToken cancellationToken);
}

public sealed class WeeklyCoachPeriodService : IWeeklyCoachPeriodService
{
    private const string DefaultTimeZoneId = "Central European Standard Time";
    private const int MaximumLookbackWeeks = 52;
    private readonly FitspireDbContext _context;

    public WeeklyCoachPeriodService(FitspireDbContext context)
    {
        _context = context;
    }

    public async Task<WeeklyCoachPeriod> GetLatestCompletedAsync(Guid userId, DateTime utcNow,
        CancellationToken cancellationToken)
    {
        var timeZoneId = await GetUserTimeZoneIdAsync(userId, cancellationToken);
        return WeeklyCoachPeriod.CreateLatestCompleted(timeZoneId, utcNow);
    }

    public async Task<WeeklyCoachPeriod> ResolveCompletedAsync(Guid userId, DateOnly? periodStart, DateTime utcNow,
        CancellationToken cancellationToken)
    {
        var timeZoneId = await GetUserTimeZoneIdAsync(userId, cancellationToken);
        var latest = WeeklyCoachPeriod.CreateLatestCompleted(timeZoneId, utcNow);
        if (!periodStart.HasValue)
            return latest;

        if (periodStart.Value.DayOfWeek != DayOfWeek.Monday || periodStart.Value > latest.PeriodStart ||
            periodStart.Value < latest.PeriodStart.AddDays(-7 * (MaximumLookbackWeeks - 1)))
        {
            throw new DomainException("Coaching reports are available only for completed Monday–Sunday weeks from the last 52 weeks.");
        }

        return WeeklyCoachPeriod.Create(periodStart.Value, timeZoneId);
    }

    private async Task<string> GetUserTimeZoneIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _context.Users.AsNoTracking().Include(candidate => candidate.AppUserPreference)
            .FirstOrDefaultAsync(candidate => candidate.Id == userId, cancellationToken)
            ?? throw new UnauthorizedAccessException("User was not found.");
        return string.IsNullOrWhiteSpace(user.AppUserPreference?.TimeZoneId)
            ? DefaultTimeZoneId
            : user.AppUserPreference.TimeZoneId;
    }
}

public sealed record WeeklyCoachPeriod(DateOnly PeriodStart, DateOnly PeriodEnd, string TimeZoneId,
    DateTime StartAtUtc, DateTime EndExclusiveAtUtc)
{
    public static WeeklyCoachPeriod CreateLatestCompleted(string timeZoneId, DateTime utcNow)
    {
        EnsureUtc(utcNow);
        var zone = ResolveTimeZone(timeZoneId);
        var localToday = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(utcNow, zone));
        var daysSinceMonday = ((int)localToday.DayOfWeek + 6) % 7;
        return Create(localToday.AddDays(-daysSinceMonday - 7), timeZoneId);
    }

    public static WeeklyCoachPeriod Create(DateOnly periodStart, string timeZoneId)
    {
        if (periodStart == DateOnly.MinValue || periodStart.DayOfWeek != DayOfWeek.Monday)
            throw new DomainException("A coaching report period must begin on Monday.");

        var zone = ResolveTimeZone(timeZoneId);
        var endExclusive = periodStart.AddDays(7);
        return new WeeklyCoachPeriod(periodStart, endExclusive.AddDays(-1), timeZoneId.Trim(),
            ToUtc(periodStart, zone), ToUtc(endExclusive, zone));
    }

    public WeeklyCoachPeriod Previous() => Create(PeriodStart.AddDays(-7), TimeZoneId);

    private static DateTime ToUtc(DateOnly date, TimeZoneInfo zone) =>
        TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(date.ToDateTime(TimeOnly.MinValue), DateTimeKind.Unspecified), zone);

    private static TimeZoneInfo ResolveTimeZone(string? timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
            throw new DomainException("User timezone is required.");
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch (TimeZoneNotFoundException exception)
        {
            throw new DomainException("User timezone is not supported.", exception);
        }
        catch (InvalidTimeZoneException exception)
        {
            throw new DomainException("User timezone is invalid.", exception);
        }
    }

    private static void EnsureUtc(DateTime value)
    {
        if (value.Kind != DateTimeKind.Utc)
            throw new DomainException("The current time must be in UTC.");
    }
}
