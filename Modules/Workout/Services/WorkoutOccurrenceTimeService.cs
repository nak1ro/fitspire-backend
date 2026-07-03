using backend.Data;
using backend.Modules.Shared.Domain;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.Workout.Services;

public interface IWorkoutOccurrenceTimeService
{
    Task<DateTime> ResolveUtcAsync(Guid userId, DateTime localOccurrence, CancellationToken cancellationToken = default);
}

public class WorkoutOccurrenceTimeService : IWorkoutOccurrenceTimeService
{
    private const string DefaultTimeZoneId = "Central European Standard Time";
    private readonly FitspireDbContext _context;

    public WorkoutOccurrenceTimeService(FitspireDbContext context)
    {
        _context = context;
    }

    public async Task<DateTime> ResolveUtcAsync(Guid userId, DateTime localOccurrence, CancellationToken cancellationToken = default)
    {
        if (localOccurrence.Kind == DateTimeKind.Utc)
            return localOccurrence;

        var timeZoneId = await _context.UserPreferences
            .Where(preference => preference.UserId == userId)
            .Select(preference => preference.TimeZoneId)
            .FirstOrDefaultAsync(cancellationToken) ?? DefaultTimeZoneId;
        var timeZone = FindTimeZone(timeZoneId);
        var unspecifiedLocalTime = DateTime.SpecifyKind(localOccurrence, DateTimeKind.Unspecified);

        if (timeZone.IsInvalidTime(unspecifiedLocalTime))
            throw new DomainException("Workout time does not exist in the user's timezone because of daylight-saving time.");

        if (!timeZone.IsAmbiguousTime(unspecifiedLocalTime))
            return TimeZoneInfo.ConvertTimeToUtc(unspecifiedLocalTime, timeZone);

        var offset = timeZone.GetAmbiguousTimeOffsets(unspecifiedLocalTime).Max();
        return new DateTimeOffset(unspecifiedLocalTime, offset).UtcDateTime;
    }

    private static TimeZoneInfo FindTimeZone(string timeZoneId)
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch (TimeZoneNotFoundException exception)
        {
            throw new DomainException("The configured user timezone is not supported.", exception);
        }
        catch (InvalidTimeZoneException exception)
        {
            throw new DomainException("The configured user timezone is invalid.", exception);
        }
    }
}
