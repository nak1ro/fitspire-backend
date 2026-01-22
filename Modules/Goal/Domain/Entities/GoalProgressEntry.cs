using backend.Modules.Shared.Domain;

namespace backend.Modules.Goal.Domain.Entities;

public class GoalProgressEntry : Entity<Guid>
{
    public Guid GoalId { get; private set; }
    public double PreviousValue { get; private set; }
    public double NewValue { get; private set; }
    public double Delta { get; private set; }
    public DateTime RecordedAt { get; private set; }
    public string? Source { get; private set; } // "workout", "manual", "nutrition"
    public Guid? SourceEntityId { get; private set; } // WorkoutId, MealId, etc.

    // Navigation
    public UserGoal Goal { get; private set; } = null!;

    private GoalProgressEntry() { }

    public GoalProgressEntry(
        Guid id,
        Guid goalId,
        double previousValue,
        double newValue,
        string? source = null,
        Guid? sourceEntityId = null)
    {
        Id = id;
        GoalId = goalId;
        PreviousValue = previousValue;
        NewValue = newValue;
        Delta = newValue - previousValue;
        Source = source;
        SourceEntityId = sourceEntityId;
        RecordedAt = DateTime.UtcNow;
        CreatedAt = DateTime.UtcNow;
    }
}
