using backend.Modules.AiCoaching.Domain;

namespace backend.Modules.AiCoaching.Services;

public interface ICoachIntentClassifier
{
    IReadOnlyList<CoachIntent> Classify(string question);
}

public sealed class CoachIntentClassifier : ICoachIntentClassifier
{
    private static readonly IReadOnlyDictionary<CoachIntent, string[]> Keywords = new Dictionary<CoachIntent, string[]>
    {
        [CoachIntent.Workout] = ["workout", "training", "train", "lift", "lifting", "run", "running", "gym", "exercise", "volume", "distance", "pace"],
        [CoachIntent.Recovery] = ["recovery", "recover", "rest", "fatigue", "tired", "sore", "sleep", "deload", "overtrain"],
        [CoachIntent.Goal] = ["goal", "target", "progress", "on track", "milestone", "streak"],
        [CoachIntent.Challenge] = ["challenge", "leaderboard", "rank", "ranking"],
        [CoachIntent.BodyProgress] = ["weight", "body fat", "waist", "measurement", "body progress", "physique"],
        [CoachIntent.Nutrition] = ["nutrition", "calorie", "calories", "macro", "protein", "carb", "fat", "meal", "diet"],
        [CoachIntent.Wellbeing] = ["mood", "energy", "wellbeing", "well-being", "motivation", "stress"]
    };

    public IReadOnlyList<CoachIntent> Classify(string question)
    {
        var normalized = question?.Trim().ToLowerInvariant() ?? string.Empty;
        var intents = Keywords.Where(pair => pair.Value.Any(keyword => normalized.Contains(keyword, StringComparison.Ordinal)))
            .Select(pair => pair.Key).OrderBy(intent => intent).ToList();

        return intents.Count == 0 ? [CoachIntent.GeneralFitness] : intents;
    }
}
