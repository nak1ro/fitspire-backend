using backend.Modules.Goal.DTOs;
using backend.Modules.Goal.Infrastructure;
using AutoMapper;
using MediatR;

namespace backend.Modules.Goal.Features;

public record GetGoalTypesQuery() : IRequest<List<GoalTypeResponse>>;

public class GetGoalTypesHandler : IRequestHandler<GetGoalTypesQuery, List<GoalTypeResponse>>
{
    private readonly IGoalRepository _repository;
    private readonly IMapper _mapper;

    public GetGoalTypesHandler(IGoalRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<List<GoalTypeResponse>> Handle(GetGoalTypesQuery request, CancellationToken cancellationToken)
    {
        var types = (await _repository.GetAllGoalTypesAsync(cancellationToken))
            .Where(type => type.IsActive && !string.IsNullOrWhiteSpace(type.MetricCode))
            .ToList();
        
        return _mapper.Map<List<GoalTypeResponse>>(types);
    }
}
