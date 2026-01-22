using backend.Data;
using backend.Modules.Workout.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.Workout.Data.Seeding;

public static class ExerciseSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FitspireDbContext>();

        if (await context.Exercises.AnyAsync())
            return; // Already seeded

        var categories = new List<ExerciseCategory>
        {
            new() { Id = Guid.NewGuid(), Name = "Chest", Description = "Exercises targeting the pectoral muscles" },
            new() { Id = Guid.NewGuid(), Name = "Back", Description = "Exercises targeting the back muscles (lats, traps, rhomboids)" },
            new() { Id = Guid.NewGuid(), Name = "Legs", Description = "Exercises targeting quadriceps, hamstrings, glutes, and calves" },
            new() { Id = Guid.NewGuid(), Name = "Shoulders", Description = "Exercises targeting the deltoids" },
            new() { Id = Guid.NewGuid(), Name = "Arms", Description = "Exercises targeting biceps and triceps" },
            new() { Id = Guid.NewGuid(), Name = "Core", Description = "Exercises targeting abs and obliques" },
            new() { Id = Guid.NewGuid(), Name = "Cardio", Description = "Cardiovascular endurance exercises" },
            new() { Id = Guid.NewGuid(), Name = "Full Body", Description = "Compound movements targeting multiple muscle groups" }
        };

        await context.ExerciseCategories.AddRangeAsync(categories);
        await context.SaveChangesAsync();

        var chest = categories.First(c => c.Name == "Chest");
        var back = categories.First(c => c.Name == "Back");
        var legs = categories.First(c => c.Name == "Legs");
        var shoulders = categories.First(c => c.Name == "Shoulders");
        var arms = categories.First(c => c.Name == "Arms");
        var core = categories.First(c => c.Name == "Core");
        var cardio = categories.First(c => c.Name == "Cardio");
        var fullBody = categories.First(c => c.Name == "Full Body");

        var exercises = new List<Exercise>
        {
            // Chest
            new() { Id = Guid.NewGuid(), Name = "Barbell Bench Press", CategoryId = chest.Id, Description = "Compound chest exercise using a barbell." },
            new() { Id = Guid.NewGuid(), Name = "Dumbbell Press", CategoryId = chest.Id, Description = "Chest press using dumbbells for better range of motion." },
            new() { Id = Guid.NewGuid(), Name = "Incline Bench Press", CategoryId = chest.Id, Description = "Targets the upper chest." },
            new() { Id = Guid.NewGuid(), Name = "Push-Ups", CategoryId = chest.Id, Description = "Bodyweight chest exercise." },
            new() { Id = Guid.NewGuid(), Name = "Cable Flyes", CategoryId = chest.Id, Description = "Isolation exercise for the chest." },
            new() { Id = Guid.NewGuid(), Name = "Dips", CategoryId = chest.Id, Description = "Compound movement for chest and triceps." },

            // Back
            new() { Id = Guid.NewGuid(), Name = "Pull-Ups", CategoryId = back.Id, Description = "Bodyweight exercise for lats." },
            new() { Id = Guid.NewGuid(), Name = "Lat Pulldown", CategoryId = back.Id, Description = "Machine exercise for lats." },
            new() { Id = Guid.NewGuid(), Name = "Deadlift", CategoryId = back.Id, Description = "Compound lift for the entire posterior chain." },
            new() { Id = Guid.NewGuid(), Name = "Bent-Over Barbell Row", CategoryId = back.Id, Description = "Rowing movement for back thickness." },
            new() { Id = Guid.NewGuid(), Name = "Seated Cable Row", CategoryId = back.Id, Description = "Horizontal pull for the mid-back." },
            new() { Id = Guid.NewGuid(), Name = "Single-Arm Dumbbell Row", CategoryId = back.Id, Description = "Unilateral back exercise." },

            // Legs
            new() { Id = Guid.NewGuid(), Name = "Barbell Squat", CategoryId = legs.Id, Description = "King of leg exercises." },
            new() { Id = Guid.NewGuid(), Name = "Leg Press", CategoryId = legs.Id, Description = "Machine based compound leg movement." },
            new() { Id = Guid.NewGuid(), Name = "Lunges", CategoryId = legs.Id, Description = "Unilateral leg exercise." },
            new() { Id = Guid.NewGuid(), Name = "Romanian Deadlift", CategoryId = legs.Id, Description = "Hinge movement for hamstrings." },
            new() { Id = Guid.NewGuid(), Name = "Leg Extensions", CategoryId = legs.Id, Description = "Isolation exercise for quadriceps." },
            new() { Id = Guid.NewGuid(), Name = "Lying Leg Curls", CategoryId = legs.Id, Description = "Isolation exercise for hamstrings." },
            new() { Id = Guid.NewGuid(), Name = "Calf Raises", CategoryId = legs.Id, Description = "Isolation exercise for calves." },

            // Shoulders
            new() { Id = Guid.NewGuid(), Name = "Overhead Barbell Press", CategoryId = shoulders.Id, Description = "Compound shoulder movement." },
            new() { Id = Guid.NewGuid(), Name = "Dumbbell Shoulder Press", CategoryId = shoulders.Id, Description = "Seated or standing shoulder press." },
            new() { Id = Guid.NewGuid(), Name = "Lateral Raises", CategoryId = shoulders.Id, Description = "Isolation for side delts." },
            new() { Id = Guid.NewGuid(), Name = "Front Raises", CategoryId = shoulders.Id, Description = "Isolation for front delts." },
            new() { Id = Guid.NewGuid(), Name = "Face Pulls", CategoryId = shoulders.Id, Description = "Rear delt and rotator cuff exercise." },
            new() { Id = Guid.NewGuid(), Name = "Reverse Machine Fly", CategoryId = shoulders.Id, Description = "Isolation for rear delts." },

            // Arms
            new() { Id = Guid.NewGuid(), Name = "Barbell Curl", CategoryId = arms.Id, Description = "Classic bicep builder." },
            new() { Id = Guid.NewGuid(), Name = "Dumbbell Curl", CategoryId = arms.Id, Description = "Unilateral bicep work." },
            new() { Id = Guid.NewGuid(), Name = "Hammer Curls", CategoryId = arms.Id, Description = "Targets the brachialis." },
            new() { Id = Guid.NewGuid(), Name = "Tricep Pushdown", CategoryId = arms.Id, Description = "Cable exercise for triceps." },
            new() { Id = Guid.NewGuid(), Name = "Skullcrushers", CategoryId = arms.Id, Description = "Extension movement for triceps." },
            new() { Id = Guid.NewGuid(), Name = "Close-Grip Bench Press", CategoryId = arms.Id, Description = "Compound tricep dominance." },

            // Core
            new() { Id = Guid.NewGuid(), Name = "Plank", CategoryId = core.Id, Description = "Isometric core hold." },
            new() { Id = Guid.NewGuid(), Name = "Russian Twists", CategoryId = core.Id, Description = "Rotational core exercise." },
            new() { Id = Guid.NewGuid(), Name = "Hanging Leg Raises", CategoryId = core.Id, Description = "Lower abs focus." },
            new() { Id = Guid.NewGuid(), Name = "Cable Woodchoppers", CategoryId = core.Id, Description = "Oblique powerhouse." },

            // Cardio/FullBody
            new() { Id = Guid.NewGuid(), Name = "Burpees", CategoryId = fullBody.Id, Description = "Full body metabolic conditioning." },
            new() { Id = Guid.NewGuid(), Name = "Kettlebell Swing", CategoryId = fullBody.Id, Description = "Hinge movement for power." },
            new() { Id = Guid.NewGuid(), Name = "Box Jumps", CategoryId = cardio.Id, Description = "Explosive leg power." },
            new() { Id = Guid.NewGuid(), Name = "Jump Rope", CategoryId = cardio.Id, Description = "Coordination and conditioning." }
        };

        // Check for duplicates before adding just in case
        foreach (var exercise in exercises)
        {
            if (!await context.Exercises.AnyAsync(e => e.Name == exercise.Name))
            {
                await context.Exercises.AddAsync(exercise);
            }
        }

        await context.SaveChangesAsync();
    }
}
