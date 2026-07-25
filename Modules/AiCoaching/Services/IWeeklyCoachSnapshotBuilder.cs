namespace backend.Modules.AiCoaching.Services;

public interface IWeeklyCoachSnapshotBuilder
{
    Task<WeeklyCoachSnapshotBuildResult> BuildAsync(Guid userId, WeeklyCoachPeriod period,
        CancellationToken cancellationToken);
}
