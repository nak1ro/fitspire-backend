using backend.Modules.User.DTOs;
using FluentValidation;

namespace backend.Modules.User.Validators;

public class AttachProfilePictureDtoValidator : AbstractValidator<AttachProfilePictureDto>
{
    public AttachProfilePictureDtoValidator()
    {
        RuleFor(dto => dto.MediaAssetId).NotEmpty();
    }
}
