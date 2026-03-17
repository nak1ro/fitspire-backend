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
