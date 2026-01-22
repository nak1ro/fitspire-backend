using backend.Modules.Goal.Domain.Entities;
using backend.Modules.Goal.Infrastructure;
using backend.Modules.Shared;
using backend.Modules.Shared.Domain;
using MediatR;

namespace backend.Modules.Goal.Features;

public record UpdateGoalProgressCommand(
    Guid GoalId,
    Guid UserId,
    double Delta,
    string? Source = null,
    Guid? SourceEntityId = null
) : IRequest;

public class UpdateGoalProgressHandler : IRequestHandler<UpdateGoalProgressCommand>
{
    private readonly IGoalRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateGoalProgressHandler(IGoalRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateGoalProgressCommand request, CancellationToken cancellationToken)
    {
        var goal = await _repository.GetByIdAsync(request.GoalId, cancellationToken);
        
        if (goal == null)
            throw new NotFoundException($"Goal {request.GoalId} not found.");

        if (goal.UserId != request.UserId)
            throw new UnauthorizedAccessException("Cannot update another user's goal.");

        var previousValue = goal.CurrentValue;
        goal.UpdateProgress(request.Delta);

        // Record history
        var entry = new GoalProgressEntry(
            Guid.NewGuid(),
            goal.Id,
            previousValue,
            goal.CurrentValue,
            request.Source,
            request.SourceEntityId
        );

        await _repository.AddProgressEntryAsync(entry, cancellationToken);
        await _repository.UpdateAsync(goal, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
