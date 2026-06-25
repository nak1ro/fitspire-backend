using backend.Modules.Badge.Domain.Constants;
using backend.Modules.Shared.Domain;

namespace backend.Modules.Badge.Services;

public sealed class BadgeAchievementSnapshot
{
    private readonly IReadOnlyDictionary<string, double> _values;

    public BadgeAchievementSnapshot(IReadOnlyDictionary<string, double> values)
    {
        _values = values;
    }

    public double GetValue(string criterionCode)
    {
        if (!_values.TryGetValue(criterionCode, out var value))
            throw new DomainException($"No badge achievement value exists for criterion '{criterionCode}'.");

        return value;
    }
}

public sealed record BadgeTriggerContext(string EntityType, Guid EntityId)
{
    public static BadgeTriggerContext ForWorkout(Guid workoutId) => new(BadgeTriggerTypes.Workout, workoutId);
    public static BadgeTriggerContext ForGoalPeriod(Guid periodId) => new(BadgeTriggerTypes.GoalPeriod, periodId);
    public static BadgeTriggerContext ForChallengeResult(Guid resultId) => new(BadgeTriggerTypes.ChallengeResult, resultId);
    public static BadgeTriggerContext ForSocialPost(Guid postId) => new(BadgeTriggerTypes.SocialPost, postId);
}
