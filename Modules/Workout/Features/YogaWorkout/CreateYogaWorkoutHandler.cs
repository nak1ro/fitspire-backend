using backend.Modules.Shared;
using backend.Modules.Workout.Domain.Entities;
using backend.Modules.Workout.Infrastructure;
using MediatR;

namespace backend.Modules.Workout.Features.YogaWorkout;

public class CreateYogaWorkoutHandler : IRequestHandler<CreateYogaWorkoutCommand, Guid>
{
    private readonly IWorkoutRepository _workoutRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateYogaWorkoutHandler(IWorkoutRepository workoutRepository, IUnitOfWork unitOfWork)
    {
        _workoutRepository = workoutRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateYogaWorkoutCommand request, CancellationToken cancellationToken)
    {
        var workout = new YogaUserWorkoutDetails(
            Guid.NewGuid(),
            request.UserId,
            request.Date,
            request.DurationMinutes,
            request.Notes
        );

        workout.SetDetails(request.Style, request.Intensity, request.FocusArea);
        workout.SetCalories(request.CaloriesBurned);
        
        if (request.IsPrivate)
            workout.SetPrivacy(true);

        await _workoutRepository.AddAsync(workout, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return workout.Id;
    }
}
