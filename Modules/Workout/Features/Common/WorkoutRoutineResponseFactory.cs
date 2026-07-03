using System.Text.Json;
using backend.Modules.Shared.Domain;
using backend.Modules.Workout.Domain.Entities;
using backend.Modules.Workout.DTOs;

namespace backend.Modules.Workout.Features.Common;

internal static class WorkoutRoutineResponseFactory
{
    public static WorkoutRoutineResponse Create(WorkoutRoutine routine)
    {
        using var document = JsonDocument.Parse(routine.RoutineDataJson);
        var definition = document.RootElement.Clone();
        if (!definition.TryGetProperty("SchemaVersion", out var schemaVersion) || !schemaVersion.TryGetInt32(out var version))
            throw new DomainException("Routine definition is missing a valid schema version.");

        return new WorkoutRoutineResponse(
            routine.Id,
            routine.Name,
            routine.Description,
            routine.WorkoutType,
            version,
            definition,
            routine.CreatedAt,
            routine.UpdatedAt);
    }
}
