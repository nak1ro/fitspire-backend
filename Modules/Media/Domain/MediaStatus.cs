using System.Text.Json.Serialization;

namespace backend.Modules.Media.Domain;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MediaStatus
{
    Pending = 1,
    Processing = 2,
    Ready = 3,
    Attached = 4,
    Rejected = 5,
    Retired = 6
}
