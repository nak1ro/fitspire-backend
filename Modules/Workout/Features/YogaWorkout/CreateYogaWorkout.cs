using backend.Modules.Shared;
using backend.Modules.Workout.Domain.Entities;
using backend.Modules.Workout.Domain.Enums;
using backend.Modules.Workout.Infrastructure;
using backend.Modules.Workout.Services;
using MediatR;

namespace backend.Modules.Workout.Features.YogaWorkout;

public record CreateYogaWorkoutCommand(
    Guid UserId,
    DateTime Date,
    YogaStyle? Style,
    YogaIntensity? Intensity,
    YogaFocusArea? FocusArea,
    double? DurationMinutes,
    int? CaloriesBurned,
    string? Notes,
    bool IsPrivate
) : IRequest<Guid>;

public class CreateYogaWorkoutHandler : IRequestHandler<CreateYogaWorkoutCommand, Guid>
{
    private readonly IWorkoutRepository _workoutRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWorkoutDerivedDataService _derivedData;

    public CreateYogaWorkoutHandler(IWorkoutRepository workoutRepository, IUnitOfWork unitOfWork, IWorkoutDerivedDataService derivedData)
    {
        _workoutRepository = workoutRepository;
        _unitOfWork = unitOfWork;
        _derivedData = derivedData;
    }

    public async Task<Guid> Handle(CreateYogaWorkoutCommand request, CancellationToken cancellationToken)
    {
        var workout = new YogaUserWorkoutDetails(
            Guid.NewGuid(),
            request.UserId,
            request.Date,
            null,
            null
        );

        workout.SetDetails(request.Style, request.Intensity, request.FocusArea);
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
