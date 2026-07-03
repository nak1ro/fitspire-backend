using backend.Data;
using backend.Modules.Progress.Services;
using backend.Modules.Workout.Domain.Entities;
using backend.Modules.Workout.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.Workout.Features.Common;

public record GetWorkoutHistoryQuery(Guid UserId, bool Archived, int Page, int PageSize) : IRequest<WorkoutPageResponse>;
public record GetActivitySummaryQuery(Guid UserId, DateTime? From, DateTime? To) : IRequest<ActivitySummaryResponse>;

public class GetWorkoutHistoryHandler : IRequestHandler<GetWorkoutHistoryQuery, WorkoutPageResponse>
{
    private readonly FitspireDbContext _context;
    public GetWorkoutHistoryHandler(FitspireDbContext context) => _context = context;

    public async Task<WorkoutPageResponse> Handle(GetWorkoutHistoryQuery request, CancellationToken cancellationToken)
    {
        var query = _context.UserWorkouts
            .Where(workout => workout.UserId == request.UserId && (request.Archived ? workout.DeletedAt != null : workout.DeletedAt == null));
        var count = await query.CountAsync(cancellationToken);
        var workouts = await query.Include(workout => ((GymUserWorkoutDetails)workout).Exercises)
                .ThenInclude(exercise => exercise.WorkoutSets)
            .OrderByDescending(workout => workout.Date).ThenByDescending(workout => workout.Id)
            .Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToListAsync(cancellationToken);
        return new WorkoutPageResponse(workouts.Select(Map).ToList(), request.Page, request.PageSize, count);
    }

    private static WorkoutHistoryItemResponse Map(UserWorkout workout) => new(
        workout.Id, workout.WorkoutType, workout.Date, workout.DurationMinutes, workout.CaloriesBurned, workout.IsPrivate,
        workout.Status.ToString(), workout.CompletedAt, workout.CreatedFromRoutineId, NotesPreview(workout.Notes), Summary(workout));

    private static string? NotesPreview(string? notes) => string.IsNullOrWhiteSpace(notes) ? null : notes.Length <= 160 ? notes : notes[..160];

    private static WorkoutHistorySummaryResponse Summary(UserWorkout workout) => workout switch
    {
        GymUserWorkoutDetails gym => new(ExerciseCount: gym.GetExerciseCount(), CompletedSetCount: gym.Exercises.Sum(e => e.WorkoutSets.Count(s => s.IsCompleted)), TotalVolumeKg: gym.GetTotalVolume(), MaximumWeightKg: gym.GetMaxWeight()),
        RunningUserWorkoutDetails running => new(DistanceKm: running.DistanceKm, ElevationGainMeters: running.ElevationGainMeters, StepCount: running.StepCount, AveragePaceMinutesPerKm: Ratio(workout.DurationMinutes, running.DistanceKm)),
        CyclingUserWorkoutDetails cycling => new(DistanceKm: cycling.DistanceKm, ElevationGainMeters: cycling.ElevationGainMeters, IsIndoor: cycling.IsIndoor, AverageSpeedKph: Ratio(cycling.DistanceKm * 60, workout.DurationMinutes)),
        SwimmingUserWorkoutDetails swimming => new(DistanceMeters: swimming.DistanceMeters, Laps: swimming.Laps, PoolLengthMeters: swimming.PoolLengthMeters, StrokeType: swimming.StrokeType?.ToString()),
        YogaUserWorkoutDetails yoga => new(Style: yoga.Style?.ToString(), Intensity: yoga.Intensity?.ToString(), FocusArea: yoga.FocusArea?.ToString()),
        _ => new()
    };

    private static double? Ratio(double? numerator, double? denominator) => numerator is > 0 && denominator is > 0 ? numerator / denominator : null;
}

public class GetActivitySummaryHandler : IRequestHandler<GetActivitySummaryQuery, ActivitySummaryResponse>
{
    private readonly FitspireDbContext _context;
    public GetActivitySummaryHandler(FitspireDbContext context) => _context = context;
    public async Task<ActivitySummaryResponse> Handle(GetActivitySummaryQuery request, CancellationToken cancellationToken)
    {
        var from = request.From?.ToUniversalTime() ?? DateTime.UtcNow.Date.AddDays(-30);
        var to = request.To?.ToUniversalTime() ?? DateTime.UtcNow;
        var contributions = _context.ActivityContributions.Where(item => item.UserId == request.UserId && item.IsActive && item.OccurredAt >= from && item.OccurredAt < to);
        async Task<double> Sum(string code) => await contributions.Where(item => item.MetricCode == code).SumAsync(item => (double?)item.Value, cancellationToken) ?? 0;
        return new ActivitySummaryResponse(from, to, (int)await Sum(MetricCatalogue.WorkoutCount), await Sum(MetricCatalogue.DurationMinutes), await Sum(MetricCatalogue.DistanceKm), await Sum(MetricCatalogue.Calories), await Sum(MetricCatalogue.GymVolumeKg));
    }
}
