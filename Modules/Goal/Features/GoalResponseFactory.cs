using backend.Modules.Goal.Domain.Entities;
using backend.Modules.Goal.Domain.Enums;
using backend.Modules.Goal.DTOs;

namespace backend.Modules.Goal.Features;

internal static class GoalResponseFactory
{
    public static GoalResponse Create(UserGoal goal)
    {
        var currentPeriod = goal.Periods.OrderByDescending(period => period.StartAt).FirstOrDefault();

        // Recurring goals surface their current period's status (e.g. a just-failed week) so the
        // parent doesn't just always read "Active". But that substitution must stop once the goal
        // itself leaves Active (e.g. archived) — the period itself isn't terminated by archiving,
        // so without this guard an archived recurring goal would still display "Active".
        var status = goal.IsRecurring && goal.Status == GoalStatus.Active && currentPeriod is not null
            ? currentPeriod.Status
            : goal.Status.ToString();
        return new GoalResponse(goal.Id, goal.GoalTypeId, goal.GoalType.Name, goal.TargetValue, goal.CurrentValue,
            goal.Unit, goal.StartDate, goal.Deadline, goal.IsRecurring, goal.RecurrencePattern, status,
            goal.IsPublic, goal.CurrentStreak, goal.GetMilestonePercent(), goal.CreatedAt);
    }
}
