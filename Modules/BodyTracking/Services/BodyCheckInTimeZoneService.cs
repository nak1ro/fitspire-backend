using backend.Data;
using backend.Modules.Shared.Domain;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.BodyTracking.Services;

public interface IBodyCheckInTimeZoneService
{
    Task<DateOnly> GetTodayAsync(Guid userId, CancellationToken cancellationToken);
}

public class BodyCheckInTimeZoneService : IBodyCheckInTimeZoneService
{
    private readonly FitspireDbContext _context;

    public BodyCheckInTimeZoneService(FitspireDbContext context)
    {
        _context = context;
    }

    public async Task<DateOnly> GetTodayAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _context.Users.Include(candidate => candidate.AppUserPreference)
            .FirstOrDefaultAsync(candidate => candidate.Id == userId, cancellationToken)
            ?? throw new UnauthorizedAccessException("User was not found.");
        var timeZoneId = user.AppUserPreference?.TimeZoneId ?? "Central European Standard Time";

        try
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone));
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
