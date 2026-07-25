using System.Text.Json;

namespace backend.Modules.AiCoaching.Services;

public static class WeeklyCoachStructuredOutputSchema
{
    public const string Name = "weekly_coach_report";
    public const string Version = "weekly-coach-report-v1";

    private const string SchemaJson = """
        {
          "type": "object",
          "additionalProperties": false,
          "properties": {
            "headline": { "type": "string", "maxLength": 120 },
            "overview": { "type": "string", "maxLength": 900 },
            "wins": { "$ref": "#/$defs/observations" },
            "patterns": { "$ref": "#/$defs/observations" },
            "nextWeekActions": { "$ref": "#/$defs/actions" },
            "dataLimitations": {
              "type": "array",
              "maxItems": 3,
              "items": { "type": "string", "maxLength": 240 }
            }
          },
          "required": ["headline", "overview", "wins", "patterns", "nextWeekActions", "dataLimitations"],
          "$defs": {
            "observation": {
              "type": "object",
              "additionalProperties": false,
              "properties": {
                "title": { "type": "string", "maxLength": 100 },
                "explanation": { "type": "string", "maxLength": 360 },
                "category": { "type": "string", "enum": ["Workout", "Consistency", "Recovery", "Nutrition", "Wellbeing", "Goal", "Challenge"] },
                "evidenceKeys": {
                  "type": "array",
                  "minItems": 1,
                  "maxItems": 4,
                  "items": { "type": "string", "maxLength": 100 }
                }
              },
              "required": ["title", "explanation", "category", "evidenceKeys"]
            },
            "observations": {
              "type": "array",
              "maxItems": 3,
              "items": { "$ref": "#/$defs/observation" }
            },
            "action": {
              "type": "object",
              "additionalProperties": false,
              "properties": {
                "title": { "type": "string", "maxLength": 100 },
                "explanation": { "type": "string", "maxLength": 360 },
                "category": { "type": "string", "enum": ["Workout", "Consistency", "Recovery", "Nutrition", "Wellbeing", "Goal", "Challenge"] },
                "evidenceKeys": {
                  "type": "array",
                  "maxItems": 4,
                  "items": { "type": "string", "maxLength": 100 }
                }
              },
              "required": ["title", "explanation", "category", "evidenceKeys"]
            },
            "actions": {
              "type": "array",
              "minItems": 1,
              "maxItems": 3,
              "items": { "$ref": "#/$defs/action" }
            }
          }
        }
        """;

    public static JsonElement Create()
    {
        using var document = JsonDocument.Parse(SchemaJson);
        return document.RootElement.Clone();
    }
}
