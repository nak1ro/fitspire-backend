using backend.Modules.Workout.DTOs;
using backend.Modules.Workout.Domain.Enums;
using FluentValidation;

namespace backend.Modules.Workout.Validators;

public class CreateGymWorkoutValidator : AbstractValidator<CreateGymWorkoutRequest>
{
    public CreateGymWorkoutValidator()
    {
        RuleFor(x => x.Date)
            .NotEmpty()
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1))
            .WithMessage("Date cannot be in the future.");

        RuleFor(x => x.Exercises)
            .NotEmpty()
            .WithMessage("At least one exercise is required.");

        RuleForEach(x => x.Exercises)
            .SetValidator(new ExerciseInputValidator());

        RuleFor(x => x.SplitType)
            .IsEnumName(typeof(WorkoutSplit), caseSensitive: false)
            .When(x => !string.IsNullOrEmpty(x.SplitType))
            .WithMessage("Invalid split type.");

        RuleFor(x => x.IntensityLevel)
            .IsEnumName(typeof(WorkoutIntensity), caseSensitive: false)
            .When(x => !string.IsNullOrEmpty(x.IntensityLevel))
            .WithMessage("Invalid intensity level.");
    }
}

public class ExerciseInputValidator : AbstractValidator<ExerciseInputRequest>
{
    public ExerciseInputValidator()
    {
        RuleFor(x => x.ExerciseId)
            .NotEmpty()
            .WithMessage("ExerciseId is required.");

        RuleFor(x => x.Sets)
            .GreaterThan(0)
            .WithMessage("Sets must be greater than 0.");

        RuleFor(x => x.Reps)
            .GreaterThan(0)
            .WithMessage("Reps must be greater than 0.");

        RuleFor(x => x.WeightKg)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Weight cannot be negative.");
    }
}

public class CompleteWorkoutValidator : AbstractValidator<CompleteWorkoutRequest>
{
    public CompleteWorkoutValidator()
    {
        RuleFor(x => x.DurationMinutes)
            .GreaterThan(0)
            .When(x => x.DurationMinutes.HasValue)
            .WithMessage("Duration must be positive.");
    }
}

public class CreateRunningWorkoutValidator : AbstractValidator<CreateRunningWorkoutRequest>
{
    public CreateRunningWorkoutValidator()
    {
        RuleFor(x => x.Date).NotEmpty().LessThanOrEqualTo(DateTime.UtcNow.AddDays(1));
        
        RuleFor(x => x.DistanceKm)
            .GreaterThan(0)
            .WithMessage("Distance must be greater than 0.");
            
        RuleFor(x => x.DurationMinutes)
            .GreaterThan(0)
            .When(x => x.DurationMinutes.HasValue)
            .WithMessage("Duration must be positive.");
            
        RuleFor(x => x.ElevationGainMeters)
            .GreaterThanOrEqualTo(0)
            .When(x => x.ElevationGainMeters.HasValue);
            
        RuleFor(x => x.StepCount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.StepCount.HasValue);
            
        RuleFor(x => x.CaloriesBurned)
            .GreaterThanOrEqualTo(0)
            .When(x => x.CaloriesBurned.HasValue);
    }
}

public class CreateCyclingWorkoutValidator : AbstractValidator<CreateCyclingWorkoutRequest>
{
    public CreateCyclingWorkoutValidator()
    {
        RuleFor(x => x.Date).NotEmpty().LessThanOrEqualTo(DateTime.UtcNow.AddDays(1));
        
        RuleFor(x => x.DistanceKm)
            .GreaterThan(0)
            .WithMessage("Distance must be greater than 0.");
            
        RuleFor(x => x.DurationMinutes)
            .GreaterThan(0)
            .When(x => x.DurationMinutes.HasValue)
            .WithMessage("Duration must be positive.");
            
        RuleFor(x => x.ElevationGainMeters)
            .GreaterThanOrEqualTo(0)
            .When(x => x.ElevationGainMeters.HasValue);
            
        RuleFor(x => x.CaloriesBurned)
            .GreaterThanOrEqualTo(0)
            .When(x => x.CaloriesBurned.HasValue);
    }
}

public class CreateSwimmingWorkoutValidator : AbstractValidator<CreateSwimmingWorkoutRequest>
{
    public CreateSwimmingWorkoutValidator()
    {
        RuleFor(x => x.Date).NotEmpty().LessThanOrEqualTo(DateTime.UtcNow.AddDays(1));

        RuleFor(x => x.Laps)
            .GreaterThan(0)
            .When(x => x.Laps.HasValue);

        RuleFor(x => x.PoolLengthMeters)
            .GreaterThan(0)
            .When(x => x.PoolLengthMeters.HasValue);

        RuleFor(x => x.DistanceMeters)
            .GreaterThan(0)
            .When(x => x.DistanceMeters.HasValue);

        RuleFor(x => x)
            .Must(x => x.DistanceMeters.HasValue || (x.Laps.HasValue && x.PoolLengthMeters.HasValue))
            .WithMessage("Provide DistanceMeters or both Laps and PoolLengthMeters.");

        RuleFor(x => x.StrokeType)
            .IsEnumName(typeof(SwimmingStroke), caseSensitive: false)
            .When(x => !string.IsNullOrWhiteSpace(x.StrokeType))
            .WithMessage("Invalid stroke type.");

        RuleFor(x => x.DurationMinutes)
            .GreaterThan(0)
            .When(x => x.DurationMinutes.HasValue)
            .WithMessage("Duration must be positive.");

        RuleFor(x => x.CaloriesBurned)
            .GreaterThanOrEqualTo(0)
            .When(x => x.CaloriesBurned.HasValue);
    }
}

public class CreateYogaWorkoutValidator : AbstractValidator<CreateYogaWorkoutRequest>
{
    public CreateYogaWorkoutValidator()
    {
        RuleFor(x => x.Date).NotEmpty().LessThanOrEqualTo(DateTime.UtcNow.AddDays(1));

        RuleFor(x => x.DurationMinutes)
            .GreaterThan(0)
            .When(x => x.DurationMinutes.HasValue)
            .WithMessage("Duration must be positive.");

        RuleFor(x => x.Style)
            .IsEnumName(typeof(YogaStyle), caseSensitive: false)
            .When(x => !string.IsNullOrWhiteSpace(x.Style))
            .WithMessage("Invalid yoga style.");

        RuleFor(x => x.Intensity)
            .IsEnumName(typeof(YogaIntensity), caseSensitive: false)
            .When(x => !string.IsNullOrWhiteSpace(x.Intensity))
            .WithMessage("Invalid yoga intensity.");

        RuleFor(x => x.FocusArea)
            .IsEnumName(typeof(YogaFocusArea), caseSensitive: false)
            .When(x => !string.IsNullOrWhiteSpace(x.FocusArea))
            .WithMessage("Invalid yoga focus area.");

        RuleFor(x => x.CaloriesBurned)
            .GreaterThanOrEqualTo(0)
            .When(x => x.CaloriesBurned.HasValue);
    }
}

public class UpdateWorkoutValidator : AbstractValidator<UpdateWorkoutRequest>
{
    public UpdateWorkoutValidator()
    {
        RuleFor(x => x.Date)
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1))
            .When(x => x.Date.HasValue)
            .WithMessage("Date cannot be in the future.");

        RuleFor(x => x.DurationMinutes)
            .GreaterThan(0)
            .When(x => x.DurationMinutes.HasValue)
            .WithMessage("Duration must be positive.");

        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .When(x => x.Notes is not null);

        RuleFor(x => x)
            .Must(x => x.Date.HasValue || x.DurationMinutes.HasValue || x.Notes is not null || x.IsPrivate.HasValue || x.CaloriesBurned.HasValue || x.DistanceKm.HasValue || x.ElevationGainMeters.HasValue || x.StepCount.HasValue || x.MapData is not null || x.IsIndoor.HasValue || x.Laps.HasValue || x.PoolLengthMeters.HasValue || x.DistanceMeters.HasValue || x.StrokeType is not null || x.Style is not null || x.Intensity is not null || x.FocusArea is not null || x.SplitType is not null || x.IntensityLevel is not null || x.Exercises is not null)
            .WithMessage("At least one workout field must be provided.");

        RuleFor(x => x.DistanceKm).GreaterThan(0).When(x => x.DistanceKm.HasValue);
        RuleFor(x => x.DistanceMeters).GreaterThan(0).When(x => x.DistanceMeters.HasValue);
        RuleFor(x => x.ElevationGainMeters).GreaterThanOrEqualTo(0).When(x => x.ElevationGainMeters.HasValue);
        RuleFor(x => x.StepCount).GreaterThanOrEqualTo(0).When(x => x.StepCount.HasValue);
        RuleFor(x => x.CaloriesBurned).GreaterThanOrEqualTo(0).When(x => x.CaloriesBurned.HasValue);
        RuleFor(x => x.Laps).GreaterThan(0).When(x => x.Laps.HasValue);
        RuleFor(x => x.PoolLengthMeters).GreaterThan(0).When(x => x.PoolLengthMeters.HasValue);
        RuleForEach(x => x.Exercises).SetValidator(new ExerciseInputValidator()).When(x => x.Exercises is not null);
    }
}

public class WorkoutFilterValidator : AbstractValidator<WorkoutFilterRequest>
{
    private static readonly HashSet<string> AllowedWorkoutTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "gym",
        "running",
        "cycling",
        "swimming",
        "yoga"
    };

    public WorkoutFilterValidator()
    {
        RuleFor(x => x)
            .Must(x => !x.From.HasValue || !x.To.HasValue || x.From.Value <= x.To.Value)
            .WithMessage("From date must be before or equal to To date.");

        RuleForEach(x => x.Types)
            .Must(type => !string.IsNullOrWhiteSpace(type) && AllowedWorkoutTypes.Contains(type))
            .When(x => x.Types is { Count: > 0 })
            .WithMessage("Invalid workout type.");
    }
}

public class SaveRoutineValidator : AbstractValidator<SaveRoutineRequest>
{
    public SaveRoutineValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(120);

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => x.Description is not null);
    }
}

public class CreateFromRoutineValidator : AbstractValidator<CreateFromRoutineRequest>
{
    public CreateFromRoutineValidator()
    {
        RuleFor(x => x.Date)
            .NotEmpty()
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1))
            .WithMessage("Date cannot be in the future.");
    }
}

public class UpdateRoutineValidator : AbstractValidator<UpdateRoutineRequest>
{
    public UpdateRoutineValidator()
    {
        RuleFor(request => request.Name).NotEmpty().MaximumLength(120);
        RuleFor(request => request.Description).MaximumLength(500).When(request => request.Description is not null);
        RuleFor(request => request.Definition.ValueKind).Equal(System.Text.Json.JsonValueKind.Object);
    }
}
