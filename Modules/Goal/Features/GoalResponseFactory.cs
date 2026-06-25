using backend.Modules.Goal.Domain.Entities;
using backend.Modules.Goal.DTOs;

namespace backend.Modules.Goal.Features;

internal static class GoalResponseFactory
{
    public static GoalResponse Create(UserGoal goal)
    {
        var currentPeriod = goal.Periods.OrderByDescending(period => period.StartAt).FirstOrDefault();
        var status = goal.IsRecurring && currentPeriod is not null ? currentPeriod.Status : goal.Status.ToString();
        return new GoalResponse(goal.Id, goal.GoalTypeId, goal.GoalType.Name, goal.TargetValue, goal.CurrentValue,
            goal.Unit, goal.StartDate, goal.Deadline, goal.IsRecurring, goal.RecurrencePattern, status,
            goal.IsPublic, goal.CurrentStreak, goal.GetMilestonePercent(), goal.CreatedAt);
    }
}
