using backend.Modules.Goal.DTOs;
using backend.Modules.Goal.Infrastructure;
using MediatR;

namespace backend.Modules.Goal.Features;

public record GetUserGoalsQuery(Guid UserId) : IRequest<List<GoalResponse>>;

public class GetUserGoalsHandler : IRequestHandler<GetUserGoalsQuery, List<GoalResponse>>
{
    private readonly IGoalRepository _repository;

    public GetUserGoalsHandler(IGoalRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<GoalResponse>> Handle(GetUserGoalsQuery request, CancellationToken cancellationToken)
    {
        var goals = await _repository.GetByUserIdAsync(request.UserId, cancellationToken);
        
        return goals.Select(g => new GoalResponse(
            g.Id,
            g.GoalTypeId,
            g.GoalType.Name,
            g.TargetValue,
            g.CurrentValue,
            g.Unit,
            g.StartDate,
            g.Deadline,
            g.IsRecurring,
            g.RecurrencePattern,
            g.Status.ToString(),
            g.IsPublic,
            g.CurrentStreak,
            g.GetMilestonePercent(),
            g.CreatedAt
        )).ToList();
    }
}
