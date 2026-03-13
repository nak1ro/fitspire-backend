using backend.Modules.Auth.DTOs;
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
            .MinimumLength(3);

        RuleFor(x => x.Password)
            .ApplyStrongPasswordRules();
    }
}
