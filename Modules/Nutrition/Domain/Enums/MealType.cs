using System.Text.Json.Serialization;

namespace backend.Modules.Nutrition.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MealType
{
    Breakfast,
    Lunch,
    Dinner,
    Snack
}
