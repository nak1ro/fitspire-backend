using System.Text.Json.Serialization;

namespace backend.Modules.Media.Domain;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MediaVariantKind
{
    Primary = 1,
    Thumbnail = 2
}
