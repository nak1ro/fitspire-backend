using backend.Modules.Media.Configuration;
using backend.Modules.Media.Contracts;
using backend.Modules.Media.Domain;
using backend.Modules.Shared.Domain;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace backend.Modules.Media.Infrastructure;

public class ImageSharpImageProcessor : IImageProcessor
{
    private const int ProfilePrimaryMaxEdge = 1024;
    private const int ProfileThumbnailMaxEdge = 256;
    private const int PostPrimaryMaxEdge = 2048;
    private const int PostThumbnailMaxEdge = 640;
    private readonly MediaStorageOptions _options;

    public ImageSharpImageProcessor(IOptions<MediaStorageOptions> options)
    {
        _options = options.Value;
    }

    public async Task<IReadOnlyList<ProcessedMediaVariant>> ProcessAsync(
        Stream source,
        MediaPurpose purpose,
        CancellationToken cancellationToken)
    {
        if (source.CanSeek)
            source.Position = 0;

        var format = await Image.DetectFormatAsync(source, cancellationToken);
        EnsureSupportedFormat(format?.Name);

        if (source.CanSeek)
            source.Position = 0;

        using var image = await Image.LoadAsync(source, cancellationToken);
        EnsureSafeImage(image);

        var (primaryMaxEdge, thumbnailMaxEdge) = GetSizing(purpose);
        return new[]
        {
            await CreateVariantAsync(image, MediaVariantKind.Primary, primaryMaxEdge, cancellationToken),
            await CreateVariantAsync(image, MediaVariantKind.Thumbnail, thumbnailMaxEdge, cancellationToken)
        };
    }

    private void EnsureSupportedFormat(string? formatName)
    {
        if (formatName is not ("JPEG" or "PNG" or "WEBP"))
            throw new DomainException("Only JPEG, PNG, and WebP images are supported.");
    }

    private void EnsureSafeImage(Image image)
    {
        if (image.Frames.Count > 1)
            throw new DomainException("Animated images are not supported.");

        var pixelCount = (long)image.Width * image.Height;
        if (image.Width <= 0 || image.Height <= 0 || pixelCount > _options.MaxDecodedPixels)
            throw new DomainException("Image dimensions exceed the supported limit.");
    }

    private static (int PrimaryMaxEdge, int ThumbnailMaxEdge) GetSizing(MediaPurpose purpose) => purpose switch
    {
        MediaPurpose.ProfilePicture => (ProfilePrimaryMaxEdge, ProfileThumbnailMaxEdge),
        MediaPurpose.PostImage => (PostPrimaryMaxEdge, PostThumbnailMaxEdge),
        MediaPurpose.BodyProgressPhoto => (PostPrimaryMaxEdge, PostThumbnailMaxEdge),
        _ => throw new DomainException("Unsupported media purpose.")
    };

    private static async Task<ProcessedMediaVariant> CreateVariantAsync(
        Image source,
        MediaVariantKind kind,
        int maxEdge,
        CancellationToken cancellationToken)
    {
        using var image = source.Clone(context =>
        {
            context.AutoOrient();
            var size = CalculateResize(source.Width, source.Height, maxEdge);
            context.Resize(size.Width, size.Height);
        });

        image.Metadata.ExifProfile = null;
        image.Metadata.IccProfile = null;
        image.Metadata.XmpProfile = null;

        await using var output = new MemoryStream();
        await image.SaveAsync(output, new WebpEncoder { Quality = 82 }, cancellationToken);
        return new ProcessedMediaVariant(kind, output.ToArray(), image.Width, image.Height, MediaPolicies.NormalizedContentType);
    }

    private static Size CalculateResize(int width, int height, int maxEdge)
    {
        if (width <= maxEdge && height <= maxEdge)
            return new Size(width, height);

        var scale = Math.Min((double)maxEdge / width, (double)maxEdge / height);
        return new Size(
            Math.Max(1, (int)Math.Round(width * scale)),
            Math.Max(1, (int)Math.Round(height * scale)));
    }
}
