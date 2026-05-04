using backend.Modules.Auth.DTOs;
using FluentValidation;

namespace backend.Modules.Auth.Validators;

public class ExternalLoginDtoValidator : AbstractValidator<ExternalLoginDto>
{
    public ExternalLoginDtoValidator()
    {
        RuleFor(x => x.Provider)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.IdToken)
            .NotEmpty();
    }
}
