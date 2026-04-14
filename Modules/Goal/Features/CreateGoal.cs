using backend.Modules.Goal.Domain.Entities;
using backend.Modules.Goal.Infrastructure;
using backend.Modules.Shared;
using backend.Modules.Shared.Domain;
using MediatR;

namespace backend.Modules.Goal.Features;

public record CreateGoalCommand(
    Guid UserId,
    Guid GoalTypeId,
    double TargetValue,
    string Unit,
    DateTime? Deadline,
    bool IsRecurring = false,
    string? RecurrencePattern = null,
    bool IsPublic = false
) : IRequest<Guid>;

public class CreateGoalHandler : IRequestHandler<CreateGoalCommand, Guid>
{
    private readonly IGoalRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateGoalHandler(IGoalRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateGoalCommand request, CancellationToken cancellationToken)
    {
        var goalType = await _repository.GetGoalTypeByIdAsync(request.GoalTypeId, cancellationToken);
        if (goalType is null)
            throw new NotFoundException($"Goal type {request.GoalTypeId} not found.");

        var goal = new UserGoal(
            Guid.NewGuid(),
            request.UserId,
            request.GoalTypeId,
            request.TargetValue,
            request.Unit,
            DateTime.UtcNow,
            request.Deadline,
            request.IsRecurring,
            request.RecurrencePattern,
            request.IsPublic
        );

        await _repository.AddAsync(goal, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return goal.Id;
    }
}
