using backend.Data;
using backend.Modules.Goal.Domain.Enums;
using backend.Modules.Social.Features.Follow;
using backend.Modules.Social.Features.Posts;
using backend.Modules.Workout.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.DemoData.Services;

public interface IDemoSocialService
{
    Task SeedAsync(Guid heroUserId, IReadOnlyList<Guid> fillerUserIds, Random random, CancellationToken cancellationToken);
}

public class DemoSocialService : IDemoSocialService
{
    private static readonly string[] TextPostCaptions =
    [
        "Feeling stronger every week.",
        "New personal best today — small wins add up.",
        "Rest day, but the plan is still on track.",
        "Back at it after a long week. Worth it.",
        "Training for the next race, one session at a time.",
    ];

    private readonly IMediator _mediator;
    private readonly FitspireDbContext _context;

    public DemoSocialService(IMediator mediator, FitspireDbContext context)
    {
        _mediator = mediator;
        _context = context;
    }

    public async Task SeedAsync(Guid heroUserId, IReadOnlyList<Guid> fillerUserIds, Random random,
        CancellationToken cancellationToken)
    {
        await BuildFollowGraphAsync(heroUserId, fillerUserIds, random, cancellationToken);
        var heroPostIds = await CreateHeroPostsAsync(heroUserId, cancellationToken);
        var fillerPostIds = await CreateFillerPostsAsync(fillerUserIds, random, cancellationToken);
        await CrossEngageAsync(heroUserId, fillerUserIds, heroPostIds, fillerPostIds, random, cancellationToken);
    }

    private async Task BuildFollowGraphAsync(Guid heroUserId, IReadOnlyList<Guid> fillerUserIds, Random random,
        CancellationToken cancellationToken)
    {
        for (var i = 0; i < fillerUserIds.Count; i++)
        {
            if (i < 7)
                await _mediator.Send(new FollowUserCommand(fillerUserIds[i], heroUserId), cancellationToken);
            if (i < 4)
                await _mediator.Send(new FollowUserCommand(heroUserId, fillerUserIds[i]), cancellationToken);
        }

        for (var i = 0; i < fillerUserIds.Count; i++)
        {
            var otherIndex = random.Next(fillerUserIds.Count);
            if (otherIndex == i) continue;
            await _mediator.Send(new FollowUserCommand(fillerUserIds[i], fillerUserIds[otherIndex]), cancellationToken);
        }
    }

    private async Task<List<Guid>> CreateHeroPostsAsync(Guid heroUserId, CancellationToken cancellationToken)
    {
        var postIds = new List<Guid>();
        foreach (var caption in TextPostCaptions)
            postIds.Add(await _mediator.Send(new CreatePostCommand(heroUserId, caption), cancellationToken));

        var workout = await _context.UserWorkouts.AsNoTracking()
            .Where(item => item.UserId == heroUserId && item.Status == WorkoutStatus.Completed && !item.IsPrivate)
            .OrderByDescending(item => item.Date).FirstOrDefaultAsync(cancellationToken);
        if (workout is not null)
            postIds.Add(await _mediator.Send(new ShareWorkoutCommand(heroUserId, workout.Id, "Great session today."),
                cancellationToken));

        var goal = await _context.Goals.AsNoTracking()
            .Where(item => item.UserId == heroUserId && item.Status == GoalStatus.Completed && item.IsPublic && !item.IsRecurring)
            .FirstOrDefaultAsync(cancellationToken);
        if (goal is not null)
            postIds.Add(await _mediator.Send(new ShareGoalCommand(heroUserId, goal.Id, "Goal achieved!"), cancellationToken));

        return postIds;
    }

    private async Task<List<Guid>> CreateFillerPostsAsync(IReadOnlyList<Guid> fillerUserIds, Random random,
        CancellationToken cancellationToken)
    {
        var postIds = new List<Guid>();
        foreach (var userId in fillerUserIds)
        {
            var postCount = random.Next(1, 3);
            for (var i = 0; i < postCount; i++)
            {
                var caption = TextPostCaptions[random.Next(TextPostCaptions.Length)];
                postIds.Add(await _mediator.Send(new CreatePostCommand(userId, caption), cancellationToken));
            }
        }
        return postIds;
    }

    private async Task CrossEngageAsync(Guid heroUserId, IReadOnlyList<Guid> fillerUserIds, List<Guid> heroPostIds,
        List<Guid> fillerPostIds, Random random, CancellationToken cancellationToken)
    {
        foreach (var postId in heroPostIds)
        {
            foreach (var userId in fillerUserIds.OrderBy(_ => random.Next()).Take(random.Next(2, 6)))
                await _mediator.Send(new LikePostCommand(userId, postId, true), cancellationToken);
            if (random.NextDouble() < 0.6)
                await _mediator.Send(new CommentOnPostCommand(fillerUserIds[random.Next(fillerUserIds.Count)], postId,
                    "Nice work!"), cancellationToken);
        }

        foreach (var postId in fillerPostIds.OrderBy(_ => random.Next()).Take(fillerPostIds.Count / 2))
            await _mediator.Send(new LikePostCommand(heroUserId, postId, true), cancellationToken);
    }
}
