namespace backend.Modules.Media.Domain;

public static class MediaPolicies
{
    public const int MaximumPostImages = 10;
    public const string NormalizedContentType = "image/webp";
    public static readonly IReadOnlySet<string> SupportedContentTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp"
    };
}
