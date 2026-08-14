namespace backend.Modules.AiCoaching.Services;

public interface ICoachContextSnapshotBuilder
{
    Task<CoachContextSnapshotBuildResult> BuildConversationAsync(Guid userId,
        CoachConversationContextRequest request, CancellationToken cancellationToken);

    Task<CoachContextSnapshotBuildResult> BuildDailyBriefingAsync(Guid userId,
        CoachDailyBriefingContextRequest request, CancellationToken cancellationToken);
}
