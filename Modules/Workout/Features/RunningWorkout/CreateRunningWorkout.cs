using backend.Modules.Shared;
using backend.Modules.Workout.Domain.Entities;
using backend.Modules.Workout.Infrastructure;
using backend.Modules.Workout.Services;
using MediatR;

namespace backend.Modules.Workout.Features.RunningWorkout;

public record CreateRunningWorkoutCommand(
    Guid UserId,
    DateTime Date,
    double DistanceKm,
    double? DurationMinutes,
    double? ElevationGainMeters,
    int? StepCount,
    int? CaloriesBurned,
    string? MapData,
    string? Notes,
    bool IsPrivate
) : IRequest<Guid>;

public class CreateRunningWorkoutHandler : IRequestHandler<CreateRunningWorkoutCommand, Guid>
{
    private readonly IWorkoutRepository _workoutRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWorkoutDerivedDataService _derivedData;
    private readonly IWorkoutOccurrenceTimeService _occurrenceTimeService;
    private readonly IWorkoutSessionGuard _sessionGuard;

    public CreateRunningWorkoutHandler(IWorkoutRepository workoutRepository, IUnitOfWork unitOfWork,
        IWorkoutDerivedDataService derivedData, IWorkoutOccurrenceTimeService occurrenceTimeService,
        IWorkoutSessionGuard sessionGuard)
    {
        _workoutRepository = workoutRepository;
        _unitOfWork = unitOfWork;
        _derivedData = derivedData;
        _occurrenceTimeService = occurrenceTimeService;
        _sessionGuard = sessionGuard;
    }

    public async Task<Guid> Handle(CreateRunningWorkoutCommand request, CancellationToken cancellationToken)
    {
        if (!request.DurationMinutes.HasValue)
            await _sessionGuard.EnsureCanStartAsync(request.UserId, cancellationToken);
        var occurredAtUtc = await _occurrenceTimeService.ResolveUtcAsync(request.UserId, request.Date, cancellationToken);
        var workout = new RunningUserWorkoutDetails(
            Guid.NewGuid(),
            request.UserId,
            occurredAtUtc,
            request.DistanceKm,
            null
        );

        workout.SetStats(request.ElevationGainMeters, request.StepCount, request.MapData);
        workout.SetCalories(request.CaloriesBurned);
        
        if (!string.IsNullOrEmpty(request.Notes))
            workout.UpdateNotes(request.Notes);
            
        if (request.IsPrivate)
            workout.SetPrivacy(true);

        if (request.DurationMinutes.HasValue)
            workout.Complete(request.DurationMinutes);

        await _workoutRepository.AddAsync(workout, cancellationToken);
        if (workout.Status == Domain.Enums.WorkoutStatus.Completed)
            await _derivedData.ReconcileCompletedWorkoutAsync(workout, cancellationToken);
        else
            await _unitOfWork.SaveChangesAsync(cancellationToken);

        return workout.Id;
    }
}
