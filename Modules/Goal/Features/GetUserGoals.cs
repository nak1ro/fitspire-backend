using backend.Data;
using backend.Modules.Goal.Domain.Enums;
using backend.Modules.Goal.DTOs;
using backend.Modules.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.Goal.Features;

public record GetUserGoalsQuery(Guid UserId, GoalListFilter Filter) : IRequest<GoalPageResponse<GoalResponse>>;
public record GetGoalDetailQuery(Guid UserId, Guid GoalId) : IRequest<GoalDetailResponse>;

public class GetUserGoalsHandler : IRequestHandler<GetUserGoalsQuery, GoalPageResponse<GoalResponse>>
{
    private readonly FitspireDbContext _context;

    public GetUserGoalsHandler(FitspireDbContext context) => _context = context;

    public async Task<GoalPageResponse<GoalResponse>> Handle(GetUserGoalsQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter;
        var query = _context.Goals.Include(goal => goal.GoalType).Include(goal => goal.Periods)
            .Where(goal => goal.UserId == request.UserId).AsQueryable();
        query = ApplyScope(query, filter.Scope);
        if (!string.IsNullOrWhiteSpace(filter.Status) && Enum.TryParse<GoalStatus>(filter.Status, true, out var status))
            query = query.Where(goal => goal.Status == status);

        var totalCount = await query.CountAsync(cancellationToken);
        var goals = await query.OrderByDescending(goal => goal.CreatedAt).Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize).ToListAsync(cancellationToken);
        return new GoalPageResponse<GoalResponse>(goals.Select(GoalResponseFactory.Create).ToList(), filter.Page, filter.PageSize, totalCount);
    }

    private static IQueryable<backend.Modules.Goal.Domain.Entities.UserGoal> ApplyScope(IQueryable<backend.Modules.Goal.Domain.Entities.UserGoal> query, string scope) =>
        scope.ToLowerInvariant() switch
        {
            "active" => query.Where(goal => goal.Status == GoalStatus.Active),
            "history" => query.Where(goal => goal.Status != GoalStatus.Active),
            _ => query
        };
}

public class GetGoalDetailHandler : IRequestHandler<GetGoalDetailQuery, GoalDetailResponse>
{
    private readonly FitspireDbContext _context;

    public GetGoalDetailHandler(FitspireDbContext context) => _context = context;

    public async Task<GoalDetailResponse> Handle(GetGoalDetailQuery request, CancellationToken cancellationToken)
    {
        var goal = await _context.Goals.Include(item => item.GoalType).Include(item => item.Periods)
            .FirstOrDefaultAsync(item => item.Id == request.GoalId && item.UserId == request.UserId, cancellationToken)
            ?? throw new NotFoundException("Goal not found.");
        var currentPeriod = goal.Periods.OrderByDescending(period => period.StartAt).FirstOrDefault();
        var response = currentPeriod is null ? null : new GoalPeriodResponse(currentPeriod.Id, currentPeriod.StartAt,
            currentPeriod.EndAt, currentPeriod.TargetValue, currentPeriod.ProgressValue, currentPeriod.Status,
            currentPeriod.CompletedAt, currentPeriod.FailedAt);
        var isActive = goal.Status == GoalStatus.Active;
        return new GoalDetailResponse(GoalResponseFactory.Create(goal), response, isActive, isActive);
    }
}
