using backend.Data;
using backend.Modules.Badge.Services;
using backend.Modules.Goal.Domain.Enums;
using backend.Modules.Goal.Infrastructure;
using backend.Modules.Shared;
using backend.Modules.Shared.Domain;
using backend.Modules.Social.Domain;
using backend.Modules.Social.Domain.Enums;
using backend.Modules.Social.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace backend.Modules.Social.Features.Posts;

public record ShareGoalCommand(Guid UserId, Guid GoalId, string? Caption = null, IReadOnlyList<Guid>? MediaAssetIds = null) : IRequest<Guid>;

public class ShareGoalHandler : IRequestHandler<ShareGoalCommand, Guid>
{
    private readonly FitspireDbContext _context;
    private readonly IGoalRepository _goalRepository;
    private readonly ISocialRepository _socialRepository;
    private readonly IBadgeEvaluationService _badges;
    private readonly IBadgeTransactionService _badgeTransactions;
    private readonly IUnitOfWork _unitOfWork;

    public ShareGoalHandler(
        FitspireDbContext context,
        IGoalRepository goalRepository,
        ISocialRepository socialRepository,
        IBadgeEvaluationService badges,
        IBadgeTransactionService badgeTransactions,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _goalRepository = goalRepository;
        _socialRepository = socialRepository;
        _badges = badges;
        _badgeTransactions = badgeTransactions;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(ShareGoalCommand request, CancellationToken cancellationToken)
    {
        var goal = await _goalRepository.GetByIdAsync(request.GoalId, cancellationToken);
        if (goal is null || goal.UserId != request.UserId)
            throw new NotFoundException($"Goal {request.GoalId} not found.");

        if (goal.IsRecurring)
            throw new DomainException("Only one-off goals can be shared.");

        if (goal.Status != GoalStatus.Completed)
            throw new DomainException("Only completed goals can be shared.");

        if (!goal.IsPublic)
            throw new DomainException("Private goals cannot be shared.");

        var existingPost = await _socialRepository.GetPostByReferenceAsync(
            PostType.GoalAchieved,
            request.GoalId,
            cancellationToken);
        if (existingPost is not null)
        {
            if (existingPost.IsModerationRemoved)
                throw new ConflictException("The existing goal share is unavailable.");

            await _badges.EvaluateAsync(request.UserId, [BadgeTriggerContext.ForSocialPost(existingPost.Id)], cancellationToken);
            return existingPost.Id;
        }

        var mediaAssetIds = request.MediaAssetIds ?? [];
        var assets = await PostMediaResolver.LoadReadyPostMediaAsync(_context, request.UserId, mediaAssetIds, cancellationToken);

        var snapshot = new GoalAchievedSnapshot(
            goal.Id,
            goal.GoalType.Name,
            goal.TargetValue,
            goal.Unit,
            goal.UpdatedAt ?? DateTime.UtcNow);
        var post = Post.CreateGoalAchievedPost(request.UserId, snapshot, request.Caption, mediaAssetIds);

        try
        {
            await _badgeTransactions.ExecuteAsync(async token =>
            {
                await _socialRepository.AddPostAsync(post, token);
                foreach (var asset in assets)
                    asset.Attach(DateTime.UtcNow);
                await _unitOfWork.SaveChangesAsync(token);
                await _badges.EvaluateAsync(request.UserId, [BadgeTriggerContext.ForSocialPost(post.Id)], token);
                await _unitOfWork.SaveChangesAsync(token);
            }, cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            _context.Entry(post).State = EntityState.Detached;
            var concurrentPost = await _socialRepository.GetPostByReferenceAsync(
                PostType.GoalAchieved,
                request.GoalId,
                cancellationToken);
            if (concurrentPost is not null)
            {
                if (concurrentPost.IsModerationRemoved)
                    throw new ConflictException("The existing goal share is unavailable.");

                await _badges.EvaluateAsync(request.UserId, [BadgeTriggerContext.ForSocialPost(concurrentPost.Id)], cancellationToken);
                return concurrentPost.Id;
            }

            throw;
        }

        return post.Id;
    }

    private static bool IsUniqueViolation(DbUpdateException exception)
    {
        return exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };
    }
}
