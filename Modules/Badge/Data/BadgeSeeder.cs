using backend.Data;
using backend.Modules.Badge.Domain;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.Badge.Data;

public static class BadgeSeeder
{
    public static async Task SeedAsync(FitspireDbContext context)
    {
        var definitions = new[]
        {
            ("workouts-1", "First step", "Complete your first workout", "WorkoutCount", 1d, "Bronze"),
            ("workouts-10", "Building momentum", "Complete ten workouts", "WorkoutCount", 10d, "Silver"),
            ("workouts-100", "Centurion", "Complete one hundred workouts", "WorkoutCount", 100d, "Gold"),
            ("challenges-1", "Challenger", "Finish your first challenge", "ChallengeFinishes", 1d, "Bronze"),
            ("challenge-wins-1", "Winner", "Win a leaderboard challenge", "ChallengeWins", 1d, "Gold")
        };
        for (var index = 0; index < definitions.Length; index++)
        {
            var (code, name, description, criterion, threshold, tier) = definitions[index];
            if (!await context.Badges.AnyAsync(badge => badge.Code == code))
                await context.Badges.AddAsync(new AchievementBadge { Id = Guid.NewGuid(), Code = code, Name = name, Description = description,
                    CriterionCode = criterion, Threshold = threshold, Tier = tier, DisplayOrder = index + 1 });
        }
        await context.SaveChangesAsync();
    }
}
