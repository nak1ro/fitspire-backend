using backend.Modules.Goal.Domain.Enums;

namespace backend.Modules.Goal.DTOs;

// Request DTOs
public record CreateGoalRequest(
    Guid GoalTypeId,
    double TargetValue,
    string Unit,
    DateTime? Deadline,
    bool IsRecurring = false,
    string? RecurrencePattern = null,
    bool IsPublic = false
);

public record UpdateGoalProgressRequest(
    double Delta,
    string? Source = null,
    Guid? SourceEntityId = null
);

// Response DTOs
public record GoalResponse(
    Guid Id,
    Guid GoalTypeId,
    string GoalTypeName,
    double TargetValue,
    double CurrentValue,
    string Unit,
    DateTime StartDate,
    DateTime? Deadline,
    bool IsRecurring,
    string? RecurrencePattern,
    string Status,
    bool IsPublic,
    int CurrentStreak,
    int MilestonePercent,
    DateTime CreatedAt
);

public record GoalTypeResponse(
    Guid Id,
    string Name,
    string? Description,
    string DefaultUnit,
    string Category,
    string MeasurementType,
    string? IconUrl,
    string? RelatedWorkoutType,
    string? RelatedMetric
);

public record GoalProgressEntryResponse(
    Guid Id,
    double PreviousValue,
    double NewValue,
    double Delta,
    DateTime RecordedAt,
    string? Source
);
