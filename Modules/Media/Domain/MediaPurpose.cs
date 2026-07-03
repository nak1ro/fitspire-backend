using System.Text.Json.Serialization;

namespace backend.Modules.Media.Domain;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MediaPurpose
{
    ProfilePicture = 1,
    PostImage = 2
}
