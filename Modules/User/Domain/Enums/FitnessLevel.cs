using System.Text.Json.Serialization;

namespace backend.Modules.User.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FitnessLevel
{
    Beginner,
    Intermediate,
    Advanced
}
