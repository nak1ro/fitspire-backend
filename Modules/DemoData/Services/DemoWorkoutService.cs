using backend.Data;
using backend.Modules.Workout.Domain.Enums;
using backend.Modules.Workout.Features.Common;
using backend.Modules.Workout.Features.CyclingWorkout;
using backend.Modules.Workout.Features.GymWorkout;
using backend.Modules.Workout.Features.RunningWorkout;
using backend.Modules.Workout.Features.SwimmingWorkout;
using backend.Modules.Workout.Features.YogaWorkout;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.DemoData.Services;

public interface IDemoWorkoutService
{
    // Generates a realistic multi-month history: a scattered base of workouts plus a forced
    // recent daily streak, so both workout-count and workout-streak badges have real data behind them.
    Task SeedRichHistoryAsync(Guid userId, DateTime nowUtc, Random random, CancellationToken cancellationToken);
    Task SeedLightHistoryAsync(Guid userId, DateTime nowUtc, Random random, CancellationToken cancellationToken);

    // Goals/challenges can't be backdated, so they only ever see activity logged after they're
    // created. These log workouts dated at the literal call-time DateTime.UtcNow (never backdated)
    // so they land inside an already-active goal/challenge period.
    Task SeedFreshGoalActivityAsync(Guid userId, Random random, CancellationToken cancellationToken);
    Task SeedFreshRunningAsync(Guid userId, Random random, CancellationToken cancellationToken);
}

public class DemoWorkoutService : IDemoWorkoutService
{
    private readonly IMediator _mediator;
    private readonly FitspireDbContext _context;

    public DemoWorkoutService(IMediator mediator, FitspireDbContext context)
    {
        _mediator = mediator;
        _context = context;
    }

    public async Task SeedRichHistoryAsync(Guid userId, DateTime nowUtc, Random random, CancellationToken cancellationToken)
    {
        var exerciseIds = await LoadExerciseIdsAsync(cancellationToken);

        // Scattered background activity over the last ~70 days before the streak block.
        for (var dayOffset = 100; dayOffset > 30; dayOffset--)
        {
            if (random.NextDouble() > 0.55) continue;
            var date = nowUtc.AddDays(-dayOffset);
            await LogOneWorkoutAsync(userId, date, random, exerciseIds, cancellationToken);
        }

        // Forced daily streak for the last 30 days — guarantees the streak badge is genuinely earned.
        for (var dayOffset = 29; dayOffset >= 0; dayOffset--)
        {
            var date = nowUtc.AddDays(-dayOffset);
            await LogOneWorkoutAsync(userId, date, random, exerciseIds, cancellationToken);
        }
    }

    public async Task SeedLightHistoryAsync(Guid userId, DateTime nowUtc, Random random, CancellationToken cancellationToken)
    {
        var exerciseIds = await LoadExerciseIdsAsync(cancellationToken);
        var count = random.Next(3, 9);
        for (var i = 0; i < count; i++)
        {
            var date = nowUtc.AddDays(-random.Next(1, 60));
            await LogOneWorkoutAsync(userId, date, random, exerciseIds, cancellationToken);
        }
    }

    public async Task SeedFreshGoalActivityAsync(Guid userId, Random random, CancellationToken cancellationToken)
    {
        var exerciseIds = await LoadExerciseIdsAsync(cancellationToken);

        // Sized to clear the workout-count (5), gym-volume (3,000 kg), and running-distance (15 km)
        // demo goal targets, while leaving cycling-distance (150 km target) visibly in progress.
        for (var i = 0; i < 3; i++)
            await LogGymWorkoutAsync(userId, DateTime.UtcNow, random, exerciseIds, cancellationToken);
        for (var i = 0; i < 3; i++)
            await _mediator.Send(new CreateRunningWorkoutCommand(userId, DateTime.UtcNow,
                Math.Round(6 + random.NextDouble() * 3, 1), 35 + random.Next(0, 20), 20 + random.Next(0, 100),
                5000 + random.Next(0, 2000), 300 + random.Next(0, 150), null, null, false), cancellationToken);
        for (var i = 0; i < 2; i++)
            await _mediator.Send(new CreateCyclingWorkoutCommand(userId, DateTime.UtcNow,
                Math.Round(15 + random.NextDouble() * 10, 1), 40 + random.Next(0, 30), 50 + random.Next(0, 150),
                300 + random.Next(0, 200), null, null, false, false), cancellationToken);
    }

    public async Task SeedFreshRunningAsync(Guid userId, Random random, CancellationToken cancellationToken) =>
        await _mediator.Send(new CreateRunningWorkoutCommand(userId, DateTime.UtcNow,
            Math.Round(4 + random.NextDouble() * 8, 1), 30 + random.Next(0, 40), 20 + random.Next(0, 100),
            4000 + random.Next(0, 3000), 250 + random.Next(0, 200), null, null, false), cancellationToken);

    private async Task<IReadOnlyList<Guid>> LoadExerciseIdsAsync(CancellationToken cancellationToken) =>
        await _context.Exercises.OrderBy(exercise => exercise.Id).Take(25).Select(exercise => exercise.Id)
            .ToListAsync(cancellationToken);

    private async Task LogOneWorkoutAsync(Guid userId, DateTime date, Random random, IReadOnlyList<Guid> exerciseIds,
        CancellationToken cancellationToken)
    {
        var roll = random.NextDouble();
        if (roll < 0.28 && exerciseIds.Count > 0)
            await LogGymWorkoutAsync(userId, date, random, exerciseIds, cancellationToken);
        else if (roll < 0.55)
            await _mediator.Send(new CreateRunningWorkoutCommand(userId, date, Math.Round(4 + random.NextDouble() * 8, 1),
                30 + random.Next(0, 45), 20 + random.Next(0, 150), 4000 + random.Next(0, 6000), 250 + random.Next(0, 300),
                null, null, false), cancellationToken);
        else if (roll < 0.72)
            await _mediator.Send(new CreateCyclingWorkoutCommand(userId, date, Math.Round(10 + random.NextDouble() * 25, 1),
                40 + random.Next(0, 60), 50 + random.Next(0, 300), 300 + random.Next(0, 400), null, null, false,
                random.NextDouble() < 0.3), cancellationToken);
        else if (roll < 0.85)
            await _mediator.Send(new CreateSwimmingWorkoutCommand(userId, date, 20 + random.Next(0, 30), 25,
                (20 + random.Next(0, 30)) * 25, "Freestyle", 30 + random.Next(0, 30), 200 + random.Next(0, 200),
                null, false), cancellationToken);
        else
            await _mediator.Send(new CreateYogaWorkoutCommand(userId, date, YogaStyle.Vinyasa,
                YogaIntensity.Medium, YogaFocusArea.FullBody,
                30 + random.Next(0, 30), 150 + random.Next(0, 100), null, false), cancellationToken);
    }

    private async Task LogGymWorkoutAsync(Guid userId, DateTime date, Random random, IReadOnlyList<Guid> exerciseIds,
        CancellationToken cancellationToken)
    {
        var chosen = exerciseIds.OrderBy(_ => random.Next()).Take(Math.Min(4, exerciseIds.Count)).ToList();
        var exercises = chosen.Select(exerciseId => new ExerciseInput(exerciseId, BuildSets(random), null)).ToList();

        var splits = new[] { "Push", "Pull", "Legs", "FullBody" };
        var workoutId = await _mediator.Send(new CreateGymWorkoutCommand(userId, date, splits[random.Next(splits.Length)],
            "Medium", exercises), cancellationToken);
        await _mediator.Send(new CompleteWorkoutCommand(workoutId, userId,
            45 + random.Next(0, 30), null, false), cancellationToken);
    }

    private static List<SetInput> BuildSets(Random random)
    {
        var baseWeight = 30 + random.Next(0, 40);
        var sets = new List<SetInput>();
        for (var i = 0; i < 3; i++)
            sets.Add(new SetInput(6 + random.Next(0, 6), baseWeight + i * 2.5, null, null, false, 6 + random.Next(0, 3), null, true));
        return sets;
    }
}
