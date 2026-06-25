using backend.Modules.Goal.Domain.Constants;
using backend.Modules.Goal.Domain.Entities;
using backend.Modules.Goal.Domain.Enums;
using backend.Modules.Shared.Domain;

namespace backend.Modules.Goal.Services;

public interface IGoalTemplatePolicy
{
    GoalCreationRules Resolve(GoalType template, string schedule, DateTime? deadline, string? selectedWorkoutType, Guid? selectedExerciseId);
}

public sealed record GoalCreationRules(
    bool IsRecurring,
    string? RecurrencePattern,
    DateTime? Deadline,
    string? SelectedWorkoutType,
    Guid? SelectedExerciseId);

public class GoalTemplatePolicy : IGoalTemplatePolicy
{
    private static readonly HashSet<string> WorkoutTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "gym", "running", "cycling", "swimming", "yoga"
    };

    public GoalCreationRules Resolve(GoalType template, string schedule, DateTime? deadline, string? selectedWorkoutType, Guid? selectedExerciseId)
    {
        if (!template.IsActive || string.IsNullOrWhiteSpace(template.MetricCode))
            throw new DomainException("This goal template is not available.");

        var normalizedSchedule = NormalizeSchedule(schedule);
        ValidateSchedule(template, normalizedSchedule, deadline);
        var workoutType = ResolveWorkoutType(template, selectedWorkoutType);
        ValidateExercise(template, selectedExerciseId);
        return new GoalCreationRules(normalizedSchedule != GoalSchedules.OneOff,
            normalizedSchedule == GoalSchedules.OneOff ? null : normalizedSchedule,
            normalizedSchedule == GoalSchedules.OneOff ? deadline!.Value.ToUniversalTime() : null,
            workoutType, selectedExerciseId);
    }

    public static IReadOnlyList<string> GetAllowedSchedules(GoalType template) =>
        template.MeasurementType == GoalMeasurementType.Cumulative
            ? [GoalSchedules.OneOff, GoalSchedules.Daily, GoalSchedules.Weekly, GoalSchedules.Monthly]
            : [GoalSchedules.OneOff];

    private static string NormalizeSchedule(string schedule)
    {
        if (string.IsNullOrWhiteSpace(schedule) || !GoalSchedules.All.Contains(schedule))
            throw new DomainException("Goal schedule must be one-off, daily, weekly, or monthly.");
        return schedule.Trim().ToLowerInvariant();
    }

    private static void ValidateSchedule(GoalType template, string schedule, DateTime? deadline)
    {
        if (schedule != GoalSchedules.OneOff && template.MeasurementType != GoalMeasurementType.Cumulative)
            throw new DomainException("Only cumulative goal templates can recur.");
        if (schedule == GoalSchedules.OneOff && (!deadline.HasValue || deadline.Value <= DateTime.UtcNow))
            throw new DomainException("One-off goals require a future deadline.");
        if (schedule != GoalSchedules.OneOff && deadline.HasValue)
            throw new DomainException("Recurring goals do not use an overall deadline.");
    }

    private static string? ResolveWorkoutType(GoalType template, string? selectedWorkoutType)
    {
        var intrinsicWorkoutType = template.RelatedWorkoutType?.ToLowerInvariant();
        if (intrinsicWorkoutType is not null)
        {
            if (!string.IsNullOrWhiteSpace(selectedWorkoutType) && !string.Equals(selectedWorkoutType, intrinsicWorkoutType, StringComparison.OrdinalIgnoreCase))
                throw new DomainException("This goal template already defines its workout type.");
            return intrinsicWorkoutType;
        }

        if (template.ParameterKind != "WorkoutType")
        {
            if (!string.IsNullOrWhiteSpace(selectedWorkoutType))
                throw new DomainException("This goal template does not accept a workout type filter.");
            return null;
        }

        if (string.IsNullOrWhiteSpace(selectedWorkoutType))
            return null;
        if (!WorkoutTypes.Contains(selectedWorkoutType))
            throw new DomainException("Goal workout type is not supported.");
        return selectedWorkoutType.Trim().ToLowerInvariant();
    }

    private static void ValidateExercise(GoalType template, Guid? selectedExerciseId)
    {
        if (template.ParameterKind == "Exercise" && !selectedExerciseId.HasValue)
            throw new DomainException("This goal template requires an exercise.");
        if (template.ParameterKind != "Exercise" && selectedExerciseId.HasValue)
            throw new DomainException("This goal template does not accept an exercise.");
    }
}

public static class GoalDefinitionKeyFactory
{
    public static string Create(GoalType template, string schedule, string? selectedWorkoutType, Guid? selectedExerciseId) =>
        string.Join('|', template.Code, schedule, selectedWorkoutType ?? "any", selectedExerciseId?.ToString("N") ?? "none");
}
