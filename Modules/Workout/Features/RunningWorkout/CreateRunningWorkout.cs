using backend.Modules.Shared;
using backend.Modules.Workout.Domain.Entities;
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

    public CreateRunningWorkoutHandler(IWorkoutRepository workoutRepository, IUnitOfWork unitOfWork)
    {
        _workoutRepository = workoutRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateRunningWorkoutCommand request, CancellationToken cancellationToken)
    {
        var workout = new RunningUserWorkoutDetails(
            Guid.NewGuid(),
            request.UserId,
            request.Date,
            request.DistanceKm,
            request.DurationMinutes
        );

        workout.SetStats(request.ElevationGainMeters, request.StepCount, request.MapData);
        workout.SetCalories(request.CaloriesBurned);
        
        if (!string.IsNullOrEmpty(request.Notes))
            workout.UpdateNotes(request.Notes);
            
        if (request.IsPrivate)
            workout.SetPrivacy(true);

        await _workoutRepository.AddAsync(workout, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return workout.Id;
    }
}
