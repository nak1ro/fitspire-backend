using backend.Modules.Goal.Domain.Enums;
using backend.Modules.Shared.Domain;

namespace backend.Modules.Goal.Domain.Entities;

public class GoalType : Entity<Guid>
{
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public string DefaultUnit { get; private set; } = null!;
    public GoalCategory Category { get; private set; }
    public GoalMeasurementType MeasurementType { get; private set; }
    public string? IconUrl { get; private set; }
    public string? RelatedWorkoutType { get; private set; } // "gym", "running", null
    public string? RelatedMetric { get; private set; } // "distance", "weight", "count"
    public Guid? RelatedExerciseId { get; private set; } // New: specific exercise link
    public string? MetricCode { get; private set; }
    public string ParameterKind { get; private set; } = "None";
    public bool IsActive { get; private set; } = true;
    public int DisplayOrder { get; private set; }

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
        Guid? relatedExerciseId = null,
        string? code = null,
        string? metricCode = null,
        string parameterKind = "None",
        int displayOrder = 0)
    {
        Id = id;
        Code = code ?? name.ToLowerInvariant().Replace(' ', '-');
        Name = name;
        DefaultUnit = defaultUnit;
        Category = category;
        MeasurementType = measurementType;
        Description = description;
        IconUrl = iconUrl;
        RelatedWorkoutType = relatedWorkoutType;
        RelatedMetric = relatedMetric;
        RelatedExerciseId = relatedExerciseId;
        MetricCode = metricCode;
        ParameterKind = parameterKind;
        DisplayOrder = displayOrder;
        CreatedAt = DateTime.UtcNow;
    }
}
