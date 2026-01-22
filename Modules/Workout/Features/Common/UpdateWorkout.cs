using backend.Modules.Shared;
using backend.Modules.Shared.Domain;
using backend.Modules.Workout.DTOs;
using backend.Modules.Workout.Infrastructure;
using MediatR;

namespace backend.Modules.Workout.Features.Common;

public record UpdateWorkoutCommand(Guid WorkoutId, Guid UserId, UpdateWorkoutRequest Request) : IRequest;

public class UpdateWorkoutHandler : IRequestHandler<UpdateWorkoutCommand>
{
    private readonly IWorkoutRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateWorkoutHandler(IWorkoutRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateWorkoutCommand request, CancellationToken cancellationToken)
    {
        var workout = await _repository.GetByIdAsync(request.WorkoutId, cancellationToken);
        
        if (workout == null)
            throw new NotFoundException($"Workout {request.WorkoutId} not found.");

        if (workout.UserId != request.UserId)
            throw new UnauthorizedAccessException("Cannot update another user's workout.");

        workout.UpdateDetails(
            request.Request.Date,
            request.Request.DurationMinutes,
            request.Request.Notes,
            request.Request.IsPrivate
        );

        await _repository.UpdateAsync(workout, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
