using backend.Modules.Auth.DTOs;
using backend.Modules.User.Domain.Constants;
using FluentValidation;

namespace backend.Modules.Auth.Validators;

public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.UserName)
            .NotEmpty()
            .Length(UserNameRules.MinimumLength, UserNameRules.MaximumLength)
            .Matches(UserNameRules.Pattern)
            .WithMessage("Username can contain only letters, numbers, and underscores.");

        RuleFor(x => x.Password)
            .ApplyStrongPasswordRules();
    }
}
