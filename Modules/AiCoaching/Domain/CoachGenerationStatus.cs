using System.Text.Json.Serialization;

namespace backend.Modules.AiCoaching.Domain;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CoachGenerationStatus
{
    Pending,
    Processing,
    Completed,
    Failed
}
