using backend.Modules.Goal.DTOs;
using backend.Modules.Goal.Infrastructure;
using MediatR;

namespace backend.Modules.Goal.Features;

public record GetGoalTypesQuery() : IRequest<List<GoalTypeResponse>>;

public class GetGoalTypesHandler : IRequestHandler<GetGoalTypesQuery, List<GoalTypeResponse>>
{
    private readonly IGoalRepository _repository;

    public GetGoalTypesHandler(IGoalRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<GoalTypeResponse>> Handle(GetGoalTypesQuery request, CancellationToken cancellationToken)
    {
        var types = (await _repository.GetAllGoalTypesAsync(cancellationToken))
            .Where(type => type.IsActive && !string.IsNullOrWhiteSpace(type.MetricCode))
            .ToList();
        
        return types.Select(t => new GoalTypeResponse(
            t.Id,
            t.Name,
            t.Description,
            t.DefaultUnit,
            t.Category.ToString(),
            t.MeasurementType.ToString(),
            t.IconUrl,
            t.RelatedWorkoutType,
            t.RelatedMetric
        )).ToList();
    }
}
