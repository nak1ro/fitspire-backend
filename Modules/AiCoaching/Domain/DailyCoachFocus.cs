using System.Text.Json.Serialization;

namespace backend.Modules.AiCoaching.Domain;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DailyCoachFocus
{
    Train,
    Recover,
    StayConsistent,
    Plan,
    Nutrition,
    Wellbeing,
    InsufficientData
}
