using backend.Data;
using backend.Modules.Shared.Domain;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.Shared.Service;

public interface IUserLocalDateResolver
{
    /// <summary>
    /// Resolves a date-only value the user entered (an "Unspecified"-kind <see cref="DateTime"/>,
    /// as produced by binding a bare "YYYY-MM-DD" request payload) to UTC using the user's saved
    /// timezone preference. A value that is already UTC-kind is returned unchanged.
    /// </summary>
    Task<DateTime> ResolveUtcAsync(Guid userId, DateTime localOccurrence, CancellationToken cancellationToken = default);
}

public class UserLocalDateResolver : IUserLocalDateResolver
{
    private const string DefaultTimeZoneId = "Central European Standard Time";
    private readonly FitspireDbContext _context;

    public UserLocalDateResolver(FitspireDbContext context)
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
            throw new DomainException("This date does not exist in your timezone because of daylight-saving time.");

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
