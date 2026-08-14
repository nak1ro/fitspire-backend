using backend.Data;
using backend.Modules.Shared.Domain;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.AiCoaching.Services;

public interface ICoachUserTimeZoneService
{
    Task<string> GetAsync(Guid userId, CancellationToken cancellationToken);
}

public sealed class CoachUserTimeZoneService : ICoachUserTimeZoneService
{
    private const string DefaultTimeZoneId = "Central European Standard Time";
    private readonly FitspireDbContext _context;

    public CoachUserTimeZoneService(FitspireDbContext context)
    {
        _context = context;
    }

    public async Task<string> GetAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _context.Users.AsNoTracking().Include(candidate => candidate.AppUserPreference)
            .FirstOrDefaultAsync(candidate => candidate.Id == userId, cancellationToken)
            ?? throw new NotFoundException("User was not found.");
        return string.IsNullOrWhiteSpace(user.AppUserPreference?.TimeZoneId)
            ? DefaultTimeZoneId
            : user.AppUserPreference.TimeZoneId;
    }
}

public static class CoachLocalDate
{
    public static DateOnly Resolve(string timeZoneId, DateTime utcNow)
    {
        if (utcNow.Kind != DateTimeKind.Utc)
            throw new DomainException("The current time must be in UTC.");
        try
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(utcNow, timeZone));
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

    public static DateTime NextStartUtc(string timeZoneId, DateOnly localDate)
    {
        try
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            var localMidnight = DateTime.SpecifyKind(localDate.AddDays(1).ToDateTime(TimeOnly.MinValue), DateTimeKind.Unspecified);
            return TimeZoneInfo.ConvertTimeToUtc(localMidnight, timeZone);
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
}
