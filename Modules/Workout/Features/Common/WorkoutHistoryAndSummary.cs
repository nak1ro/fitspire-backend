using AutoMapper;
using backend.Data;
using backend.Modules.Progress.Services;
using backend.Modules.Workout.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.Workout.Features.Common;

public record GetWorkoutHistoryQuery(Guid UserId, bool Archived, int Page, int PageSize) : IRequest<WorkoutPageResponse>;
public record GetActivitySummaryQuery(Guid UserId, DateTime? From, DateTime? To) : IRequest<ActivitySummaryResponse>;

public class GetWorkoutHistoryHandler : IRequestHandler<GetWorkoutHistoryQuery, WorkoutPageResponse>
{
    private readonly FitspireDbContext _context; private readonly IMapper _mapper;
    public GetWorkoutHistoryHandler(FitspireDbContext context, IMapper mapper) { _context = context; _mapper = mapper; }
    public async Task<WorkoutPageResponse> Handle(GetWorkoutHistoryQuery request, CancellationToken cancellationToken)
    {
        var query = _context.UserWorkouts.Where(workout => workout.UserId == request.UserId && (request.Archived ? workout.DeletedAt != null : workout.DeletedAt == null));
        var count = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(workout => workout.Date).Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToListAsync(cancellationToken);
        return new WorkoutPageResponse(_mapper.Map<List<WorkoutResponse>>(items), request.Page, request.PageSize, count);
    }
}

public class GetActivitySummaryHandler : IRequestHandler<GetActivitySummaryQuery, ActivitySummaryResponse>
{
    private readonly FitspireDbContext _context; public GetActivitySummaryHandler(FitspireDbContext context) => _context = context;
    public async Task<ActivitySummaryResponse> Handle(GetActivitySummaryQuery request, CancellationToken cancellationToken)
    {
        var from = request.From?.ToUniversalTime() ?? DateTime.UtcNow.Date.AddDays(-30);
        var to = request.To?.ToUniversalTime() ?? DateTime.UtcNow;
        var contributions = _context.ActivityContributions.Where(item => item.UserId == request.UserId && item.IsActive && item.OccurredAt >= from && item.OccurredAt < to);
        async Task<double> Sum(string code) => await contributions.Where(item => item.MetricCode == code).SumAsync(item => (double?)item.Value, cancellationToken) ?? 0;
        return new ActivitySummaryResponse(from, to, (int)await Sum(MetricCatalogue.WorkoutCount), await Sum(MetricCatalogue.DurationMinutes), await Sum(MetricCatalogue.DistanceKm), await Sum(MetricCatalogue.Calories), await Sum(MetricCatalogue.GymVolumeKg));
    }
}
