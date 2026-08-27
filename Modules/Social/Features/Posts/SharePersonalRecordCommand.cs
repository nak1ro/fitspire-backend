using backend.Data;
using backend.Modules.Badge.Services;
using backend.Modules.Shared;
using backend.Modules.Shared.Domain;
using backend.Modules.Social.Domain;
using backend.Modules.Social.Domain.Enums;
using backend.Modules.Social.Infrastructure;
using backend.Modules.Workout.Infrastructure;
using backend.Modules.Workout.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace backend.Modules.Social.Features.Posts;

public record SharePersonalRecordCommand(Guid UserId, Guid PersonalRecordId, string? Caption = null, IReadOnlyList<Guid>? MediaAssetIds = null) : IRequest<Guid>;

public class SharePersonalRecordHandler : IRequestHandler<SharePersonalRecordCommand, Guid>
{
    private readonly FitspireDbContext _context;
    private readonly IWorkoutRepository _workoutRepository;
    private readonly ISocialRepository _socialRepository;
    private readonly IBadgeEvaluationService _badges;
    private readonly IBadgeTransactionService _badgeTransactions;
    private readonly IUnitOfWork _unitOfWork;

    public SharePersonalRecordHandler(
        FitspireDbContext context,
        IWorkoutRepository workoutRepository,
        ISocialRepository socialRepository,
        IBadgeEvaluationService badges,
        IBadgeTransactionService badgeTransactions,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _workoutRepository = workoutRepository;
        _socialRepository = socialRepository;
        _badges = badges;
        _badgeTransactions = badgeTransactions;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(SharePersonalRecordCommand request, CancellationToken cancellationToken)
    {
        var records = await _workoutRepository.GetPersonalRecordsByUserIdAsync(request.UserId, cancellationToken);
        var record = records.FirstOrDefault(r => r.Id == request.PersonalRecordId);
        if (record is null)
            throw new NotFoundException($"Personal record {request.PersonalRecordId} not found.");

        var existingPost = await _socialRepository.GetPersonalRecordSharePostAsync(record.Id, record.AchievedAt, cancellationToken);
        if (existingPost is not null)
        {
            if (existingPost.IsModerationRemoved)
                throw new ConflictException("The existing personal record share is unavailable.");

            await _badges.EvaluateAsync(request.UserId, [BadgeTriggerContext.ForSocialPost(existingPost.Id)], cancellationToken);
            return existingPost.Id;
        }

        var mediaAssetIds = request.MediaAssetIds ?? [];
        var assets = await PostMediaResolver.LoadReadyPostMediaAsync(_context, request.UserId, mediaAssetIds, cancellationToken);

        var snapshot = new PersonalRecordAchievedSnapshot(
            record.Id,
            record.WorkoutType,
            record.Metric,
            record.ExerciseId,
            record.Exercise?.Name,
            record.Value,
            PersonalRecordMetricCatalogue.Units.GetValueOrDefault(record.Metric, string.Empty),
            record.AchievedAt);
        var post = Post.CreatePersonalRecordAchievedPost(request.UserId, snapshot, request.Caption, mediaAssetIds);

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
            var concurrentPost = await _socialRepository.GetPersonalRecordSharePostAsync(record.Id, record.AchievedAt, cancellationToken);
            if (concurrentPost is not null)
            {
                if (concurrentPost.IsModerationRemoved)
                    throw new ConflictException("The existing personal record share is unavailable.");

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
