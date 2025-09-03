namespace backend.Modules.Shared.Constants;

public static class FileUploadConstants
{
    public const long MaxProfilePictureSize = 5 * 1024 * 1024; // 5MB
    public static readonly string[] AllowedProfilePictureTypes = { "image/jpeg", "image/png", "image/webp" };
    public static readonly string[] AllowedProfilePictureExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
}