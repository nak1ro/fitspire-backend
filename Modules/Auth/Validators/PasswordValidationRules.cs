using FluentValidation;

namespace backend.Modules.Auth.Validators;

public static class PasswordValidationRules
{
    public static IRuleBuilderOptions<T, string> ApplyStrongPasswordRules<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .MinimumLength(6)
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.");
    }
}
