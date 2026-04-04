using backend.Modules.Shared;
using backend.Modules.Workout.Domain.Entities;
using backend.Modules.Workout.Features.Common;
using backend.Modules.Workout.Infrastructure;
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
    private readonly IPublisher _publisher;

    public CreateRunningWorkoutHandler(IWorkoutRepository workoutRepository, IUnitOfWork unitOfWork, IPublisher publisher)
    {
        _workoutRepository = workoutRepository;
        _unitOfWork = unitOfWork;
        _publisher = publisher;
    }

    public async Task<Guid> Handle(CreateRunningWorkoutCommand request, CancellationToken cancellationToken)
    {
        var workout = new RunningUserWorkoutDetails(
            Guid.NewGuid(),
            request.UserId,
            request.Date,
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

        var completionEvents = WorkoutDomainEvents.PullCompletionEvents(workout);

        await _workoutRepository.AddAsync(workout, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await WorkoutDomainEvents.PublishAsync(_publisher, completionEvents, cancellationToken);

        return workout.Id;
    }
}
