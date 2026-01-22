using backend.Modules.Goal.Domain.Enums;
using backend.Modules.Shared.Domain;

namespace backend.Modules.Goal.Domain.Entities;

public class GoalType : Entity<Guid>
{
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public string DefaultUnit { get; private set; } = null!;
    public GoalCategory Category { get; private set; }
    public GoalMeasurementType MeasurementType { get; private set; }
    public string? IconUrl { get; private set; }
    public string? RelatedWorkoutType { get; private set; } // "gym", "running", null
    public string? RelatedMetric { get; private set; } // "distance", "weight", "count"
    public Guid? RelatedExerciseId { get; private set; } // New: specific exercise link

    // Navigation
    public ICollection<UserGoal> Goals { get; private set; } = new List<UserGoal>();

    private GoalType() { }

    public GoalType(
        Guid id,
        string name,
        string defaultUnit,
        GoalCategory category,
        GoalMeasurementType measurementType,
        string? description = null,
        string? iconUrl = null,
        string? relatedWorkoutType = null,
        string? relatedMetric = null,
        Guid? relatedExerciseId = null)
    {
        Id = id;
        Name = name;
        DefaultUnit = defaultUnit;
        Category = category;
        MeasurementType = measurementType;
        Description = description;
        IconUrl = iconUrl;
        RelatedWorkoutType = relatedWorkoutType;
        RelatedMetric = relatedMetric;
        RelatedExerciseId = relatedExerciseId;
        CreatedAt = DateTime.UtcNow;
    }
}
