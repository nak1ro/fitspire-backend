using backend.Data;
using backend.Modules.Goal.Domain.Constants;
using backend.Modules.Goal.Domain.Enums;
using backend.Modules.Goal.Features;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.DemoData.Services;

public interface IDemoGoalService
{
    Task SeedAsync(Guid userId, DateTime nowUtc, CancellationToken cancellationToken);
}

// Goals cannot be backdated — UserGoal creation rejects any StartDate in the past, so a goal can
// only ever track activity logged after it was created. Targets here are sized to be satisfied by
// the small "fresh" workout burst DemoWorkoutService logs right after these goals are created.
public class DemoGoalService : IDemoGoalService
{
    private readonly IMediator _mediator;
    private readonly FitspireDbContext _context;

    public DemoGoalService(IMediator mediator, FitspireDbContext context)
    {
        _mediator = mediator;
        _context = context;
    }

    public async Task SeedAsync(Guid userId, DateTime nowUtc, CancellationToken cancellationToken)
    {
        var deadline = nowUtc.AddDays(14);

        await CreateIfMissingAsync("workout-count", 5, deadline, cancellationToken);
        await CreateIfMissingAsync("gym-volume", 3_000, deadline, cancellationToken);
        await CreateIfMissingAsync("running-distance", 15, deadline, cancellationToken);
        // Deliberately ambitious so it stays visibly "in progress" for the demo.
        await CreateIfMissingAsync("cycling-distance", 150, deadline, cancellationToken);

        async Task CreateIfMissingAsync(string code, double targetValue, DateTime goalDeadline, CancellationToken token)
        {
            var goalTypeId = await _context.GoalTypes.Where(type => type.Code == code).Select(type => type.Id)
                .FirstOrDefaultAsync(token);
            if (goalTypeId == Guid.Empty) return;

            // Retry-safe: a prior partial run may already have created this goal for this user.
            var alreadyExists = await _context.Goals.AnyAsync(goal => goal.UserId == userId &&
                goal.GoalTypeId == goalTypeId && goal.Status == GoalStatus.Active, token);
            if (alreadyExists) return;

            // StartDate is omitted so the handler resolves it to DateTime.UtcNow at creation time —
            // passing anything backdated here throws "Goal start date cannot be in the past."
            await _mediator.Send(new CreateGoalCommand(userId, goalTypeId, targetValue, GoalSchedules.OneOff,
                goalDeadline, IsPublic: true), token);
        }
    }
}
