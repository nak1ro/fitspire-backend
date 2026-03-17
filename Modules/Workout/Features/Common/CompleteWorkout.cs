using backend.Modules.Shared;
using backend.Modules.Shared.Domain;
using backend.Modules.Workout.Infrastructure;
using MediatR;

namespace backend.Modules.Workout.Features.Common;

public record CompleteWorkoutCommand(
    Guid WorkoutId,
    Guid UserId,
    double? DurationMinutes
) : IRequest<bool>;

public class CompleteWorkoutHandler : IRequestHandler<CompleteWorkoutCommand, bool>
{
    private readonly IWorkoutRepository _workoutRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublisher _publisher;

    public CompleteWorkoutHandler(IWorkoutRepository workoutRepository, IUnitOfWork unitOfWork, IPublisher publisher)
    {
        _workoutRepository = workoutRepository;
        _unitOfWork = unitOfWork;
        _publisher = publisher;
    }

    public async Task<bool> Handle(CompleteWorkoutCommand request, CancellationToken cancellationToken)
    {
        var workout = await _workoutRepository.GetByIdAsync(request.WorkoutId, cancellationToken);
        
        if (workout is null)
            throw new DomainException($"Workout {request.WorkoutId} not found.");

        if (workout.UserId != request.UserId)
            throw new UnauthorizedAccessException("Workout does not belong to the current user.");

        workout.Complete(request.DurationMinutes);

        var domainEvents = workout.DomainEvents.ToList();
        workout.ClearDomainEvents();

        await _workoutRepository.UpdateAsync(workout, cancellationToken);

        foreach (var domainEvent in domainEvents)
        {
            await _publisher.Publish(domainEvent, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
