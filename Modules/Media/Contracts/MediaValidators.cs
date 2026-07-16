using backend.Modules.Media.Configuration;
using backend.Modules.Media.Domain;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace backend.Modules.Media.Contracts;

public class InitiateMediaUploadRequestValidator : AbstractValidator<InitiateMediaUploadRequest>
{
    public InitiateMediaUploadRequestValidator(IOptions<MediaStorageOptions> options)
    {
        var settings = options.Value;

        RuleFor(request => request.Purpose).IsInEnum();
        RuleFor(request => request.FileName).NotEmpty().MaximumLength(255);
        RuleFor(request => request.ContentType)
            .NotEmpty()
            .Must(MediaPolicies.SupportedContentTypes.Contains)
            .WithMessage("Only JPEG, PNG, and WebP images are supported.");
        RuleFor(request => request.SizeBytes)
            .GreaterThan(0)
            .Must((request, size) => size <= GetMaximumSize(request.Purpose, settings))
            .WithMessage("Image size exceeds the configured limit for this purpose.");
        RuleFor(request => request.ClientRequestId).MaximumLength(128).When(request => request.ClientRequestId is not null);
    }

    private static long GetMaximumSize(MediaPurpose purpose, MediaStorageOptions settings) => purpose switch
    {
        MediaPurpose.ProfilePicture => settings.ProfilePictureMaxBytes,
        MediaPurpose.PostImage => settings.PostImageMaxBytes,
        MediaPurpose.BodyProgressPhoto => settings.BodyProgressPhotoMaxBytes,
        _ => 0
    };
}
