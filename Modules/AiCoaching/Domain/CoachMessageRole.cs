using System.Text.Json.Serialization;

namespace backend.Modules.AiCoaching.Domain;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CoachMessageRole
{
    User,
    Assistant
}
