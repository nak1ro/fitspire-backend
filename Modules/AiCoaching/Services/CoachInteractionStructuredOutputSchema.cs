using System.Text.Json;

namespace backend.Modules.AiCoaching.Services;

public static class CoachInteractionStructuredOutputSchema
{
    public const string ConversationName = "coach_conversation_answer";
    public const string ConversationVersion = "coach-conversation-output-v1";
    public const string DailyBriefingName = "coach_daily_briefing";
    public const string DailyBriefingVersion = "coach-daily-briefing-output-v1";

    private const string ConversationSchemaJson = """
        {
          "type": "object",
          "additionalProperties": false,
          "properties": {
            "answerMarkdown": { "type": "string", "maxLength": 2200 },
            "suggestedActions": { "$ref": "#/$defs/actions" },
            "dataLimitations": { "$ref": "#/$defs/limitations" },
            "evidenceKeys": { "$ref": "#/$defs/evidenceKeys" },
            "updatedThreadSummary": { "type": "string", "maxLength": 1400 },
            "safetyCategory": { "type": "string", "enum": ["None", "GeneralCaution", "MedicalBoundary"] }
          },
          "required": ["answerMarkdown", "suggestedActions", "dataLimitations", "evidenceKeys", "updatedThreadSummary", "safetyCategory"],
          "$defs": {
            "action": {
              "type": "object",
              "additionalProperties": false,
              "properties": {
                "title": { "type": "string", "maxLength": 100 },
                "description": { "type": "string", "maxLength": 360 },
                "category": { "type": "string", "enum": ["Workout", "Recovery", "Consistency", "Nutrition", "Wellbeing", "Goal", "Challenge", "GeneralFitness"] },
                "evidenceKeys": { "$ref": "#/$defs/evidenceKeys" }
              },
              "required": ["title", "description", "category", "evidenceKeys"]
            },
            "actions": {
              "type": "array",
              "maxItems": 3,
              "items": { "$ref": "#/$defs/action" }
            },
            "limitations": {
              "type": "array",
              "maxItems": 3,
              "items": { "type": "string", "maxLength": 240 }
            },
            "evidenceKeys": {
              "type": "array",
              "maxItems": 6,
              "items": { "type": "string", "maxLength": 100 }
            }
          }
        }
        """;

    private const string DailyBriefingSchemaJson = """
        {
          "type": "object",
          "additionalProperties": false,
          "properties": {
            "headline": { "type": "string", "maxLength": 120 },
            "focus": { "type": "string", "enum": ["Train", "Recover", "StayConsistent", "Plan", "Nutrition", "Wellbeing", "InsufficientData"] },
            "summaryMarkdown": { "type": "string", "maxLength": 1200 },
            "nextAction": { "$ref": "#/$defs/action" },
            "insightMarkdown": { "type": "string", "maxLength": 700 },
            "dataLimitations": { "$ref": "#/$defs/limitations" },
            "evidenceKeys": { "$ref": "#/$defs/evidenceKeys" }
          },
          "required": ["headline", "focus", "summaryMarkdown", "nextAction", "insightMarkdown", "dataLimitations", "evidenceKeys"],
          "$defs": {
            "action": {
              "type": "object",
              "additionalProperties": false,
              "properties": {
                "title": { "type": "string", "maxLength": 100 },
                "description": { "type": "string", "maxLength": 360 },
                "category": { "type": "string", "enum": ["Workout", "Recovery", "Consistency", "Nutrition", "Wellbeing", "Goal", "Challenge", "GeneralFitness"] },
                "evidenceKeys": { "$ref": "#/$defs/evidenceKeys" }
              },
              "required": ["title", "description", "category", "evidenceKeys"]
            },
            "limitations": {
              "type": "array",
              "maxItems": 3,
              "items": { "type": "string", "maxLength": 240 }
            },
            "evidenceKeys": {
              "type": "array",
              "maxItems": 6,
              "items": { "type": "string", "maxLength": 100 }
            }
          }
        }
        """;

    public static JsonElement CreateConversation()
    {
        using var document = JsonDocument.Parse(ConversationSchemaJson);
        return document.RootElement.Clone();
    }

    public static JsonElement CreateDailyBriefing()
    {
        using var document = JsonDocument.Parse(DailyBriefingSchemaJson);
        return document.RootElement.Clone();
    }
}
