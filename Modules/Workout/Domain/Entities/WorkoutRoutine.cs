using backend.Modules.Shared.Domain;
using backend.Modules.User.Domain;

namespace backend.Modules.Workout.Domain.Entities;

public class WorkoutRoutine : AggregateRoot<Guid>
{
    public Guid UserId { get; private set; }
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public string WorkoutType { get; private set; } = null!; // "gym", "running" etc.
    
    // Stores the JSON representation of the workout creation command or data
    public string RoutineDataJson { get; private set; } = null!; 
    
    // Navigation
    public AppUser User { get; private set; } = null!;

    private WorkoutRoutine() { }

    public WorkoutRoutine(Guid id, Guid userId, string name, string workoutType, string routineDataJson, string? description)
    {
        Id = id;
        UserId = userId;
        Name = name;
        WorkoutType = workoutType;
        RoutineDataJson = routineDataJson;
        Description = description;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(string name, string? description, string routineDataJson)
    {
        Name = name;
        Description = description;
        RoutineDataJson = routineDataJson;
        UpdatedAt = DateTime.UtcNow;
    }
}
