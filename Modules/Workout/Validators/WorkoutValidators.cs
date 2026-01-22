using backend.Modules.Workout.DTOs;
using FluentValidation;

namespace backend.Modules.Workout.Validators;

public class CreateGymWorkoutValidator : AbstractValidator<CreateGymWorkoutRequest>
{
    public CreateGymWorkoutValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required.");

        RuleFor(x => x.Date)
            .NotEmpty()
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1))
            .WithMessage("Date cannot be in the future.");

        RuleFor(x => x.Exercises)
            .NotEmpty()
            .WithMessage("At least one exercise is required.");

        RuleForEach(x => x.Exercises)
            .SetValidator(new ExerciseInputValidator());
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
