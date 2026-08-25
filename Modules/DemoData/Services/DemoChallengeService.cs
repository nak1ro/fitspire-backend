using backend.Modules.Challenge.Contracts;
using backend.Modules.Challenge.Domain.Constants;
using backend.Modules.Challenge.Features;
using backend.Modules.Challenge.Services;
using backend.Modules.Progress.Services;
using MediatR;

namespace backend.Modules.DemoData.Services;

public interface IDemoChallengeService
{
    Task SeedAsync(Guid heroUserId, IReadOnlyList<Guid> fillerUserIds, Random random, CancellationToken cancellationToken);
}

// Challenges can't be backdated either (UserChallenge.Create rejects a past StartDate), so this
// seeds a currently-running challenge — started now, ending in two weeks — rather than a finished
// one. Participants get a small burst of fresh (post-join) running activity so the leaderboard
// actually shows non-zero, varied scores instead of everyone sitting at zero.
public class DemoChallengeService : IDemoChallengeService
{
    private readonly IMediator _mediator;
    private readonly IChallengeScoringService _scoring;
    private readonly IDemoWorkoutService _workouts;

    public DemoChallengeService(IMediator mediator, IChallengeScoringService scoring, IDemoWorkoutService workouts)
    {
        _mediator = mediator;
        _scoring = scoring;
        _workouts = workouts;
    }

    public async Task SeedAsync(Guid heroUserId, IReadOnlyList<Guid> fillerUserIds, Random random,
        CancellationToken cancellationToken)
    {
        var startDate = DateTime.UtcNow;
        var request = new CreateChallengeRequest("30-Day Running Challenge", "Most kilometers run this month wins.",
            MetricCatalogue.RunningDistanceKm, "running", ChallengeModes.Leaderboard, null, ChallengeVisibilities.Public,
            startDate, startDate.AddDays(14), ChallengeJoinClosingModes.AtEnd, 20);
        var challengeId = await _mediator.Send(new CreateChallengeCommand(heroUserId, request), cancellationToken);

        var participants = fillerUserIds.Take(4).ToList();
        foreach (var participantId in participants)
            await _mediator.Send(new JoinChallengeCommand(participantId, challengeId), cancellationToken);

        // Give the hero and a couple of participants some post-join running activity so the
        // leaderboard shows real, varied scores instead of everyone sitting at zero.
        await _workouts.SeedFreshRunningAsync(heroUserId, random, cancellationToken);
        foreach (var participantId in participants.Take(2))
            await _workouts.SeedFreshRunningAsync(participantId, random, cancellationToken);

        // The lifecycle background job (Upcoming -> Active, leaderboard recalculation) only runs
        // while the app is serving requests — this script exits before that, so it's driven
        // manually here to flip the just-created challenge from Upcoming to Active.
        await _scoring.ProcessLifecycleAsync(DateTime.UtcNow, cancellationToken);
    }
}
