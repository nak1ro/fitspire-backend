using backend.Data;
using backend.Modules.Social.Services;
using backend.Modules.Workout.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.Social.Features.Profiles;

public record PublicPersonalRecordResponse(
    Guid Id,
    string WorkoutType,
    string Metric,
    string Unit,
    double Value,
    string? ExerciseName,
    DateTime AchievedAt);

public record GetFeaturedPersonalRecordQuery(Guid ViewerId, Guid OwnerId) : IRequest<PublicPersonalRecordResponse?>;

public class GetFeaturedPersonalRecordHandler : IRequestHandler<GetFeaturedPersonalRecordQuery, PublicPersonalRecordResponse?>
{
    private readonly FitspireDbContext _context;
    private readonly ISocialAccessService _access;

    public GetFeaturedPersonalRecordHandler(FitspireDbContext context, ISocialAccessService access)
    {
        _context = context;
        _access = access;
    }

    public async Task<PublicPersonalRecordResponse?> Handle(GetFeaturedPersonalRecordQuery request, CancellationToken cancellationToken)
    {
        if (!await _access.CanViewProtectedContentAsync(request.ViewerId, request.OwnerId, cancellationToken))
            throw new UnauthorizedAccessException("This profile is private.");

        var record = await _context.PersonalRecords.AsNoTracking().Include(pr => pr.Exercise)
            .FirstOrDefaultAsync(pr => pr.UserId == request.OwnerId && pr.IsFeatured, cancellationToken);
        if (record is null)
            return null;

        var unit = PersonalRecordMetricCatalogue.Units.GetValueOrDefault(record.Metric, "count");
        return new PublicPersonalRecordResponse(record.Id, record.WorkoutType, record.Metric, unit, record.Value,
            record.Exercise?.Name, record.AchievedAt);
    }
}
