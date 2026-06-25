using backend.Modules.Goal.Domain.Enums;

namespace backend.Modules.Goal.DTOs;

// Request DTOs
public record CreateGoalRequest(
    Guid GoalTypeId,
    double TargetValue,
    string Schedule,
    DateTime? Deadline,
    bool IsPublic = false,
    string? SelectedWorkoutType = null,
    Guid? SelectedExerciseId = null,
    DateTime? StartDate = null
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

public record GoalDetailResponse(GoalResponse Goal, GoalPeriodResponse? CurrentPeriod, bool CanEdit, bool CanArchive);

public record GoalPageResponse<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);

public record GoalListFilter(string Scope = "active", string? Status = null, int Page = 1, int PageSize = 20);

public record GoalPagination(int Page = 1, int PageSize = 20);

public record GoalPeriodResponse(Guid Id, DateTime StartAt, DateTime EndAt, double TargetValue, double ProgressValue, string Status, DateTime? CompletedAt, DateTime? FailedAt);

public record UpdateGoalRequest(double TargetValue, bool IsPublic, DateTime? Deadline = null);

public record GoalTypeResponse(
    Guid Id,
    string Name,
    string? Description,
    string DefaultUnit,
    string Category,
    string MeasurementType,
    string? IconUrl,
    string? RelatedWorkoutType,
    string? RelatedMetric,
    string Code,
    string? MetricCode,
    string ParameterKind,
    IReadOnlyList<string> AllowedSchedules
);

public record GoalProgressEntryResponse(
    Guid Id,
    double PreviousValue,
    double NewValue,
    double Delta,
    DateTime RecordedAt,
    string? Source
);

public record GoalTargetChangeResponse(Guid Id, double PreviousTargetValue, double NewTargetValue, DateTime ChangedAt);
