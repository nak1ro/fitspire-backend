using System.Text.Json;
using backend.Modules.AiCoaching.Contracts;
using backend.Modules.AiCoaching.Domain;

namespace backend.Modules.AiCoaching.Services;

public interface ICoachInteractionResponseFactory
{
    CoachMessageResponse CreateMessage(CoachMessage message);
    DailyCoachBriefingResponse CreateDailyBriefing(DailyCoachBriefing briefing);
}

public sealed class CoachInteractionResponseFactory : ICoachInteractionResponseFactory
{
    private const string WellnessDisclaimer = "Fitspire AI provides general fitness and wellness guidance, not medical advice.";
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public CoachMessageResponse CreateMessage(CoachMessage message)
    {
        var answer = message.Role == CoachMessageRole.Assistant ? DeserializeAnswer(message.AnswerJson) : null;
        return new CoachMessageResponse(message.Id, message.SequenceNumber, message.Role, message.Status,
            message.Role == CoachMessageRole.User ? message.Question : null, answer, message.RequestedAt,
            message.ProcessingStartedAt, message.CompletedAt, message.FailedAt, message.LastFailureMessage,
            message.Role == CoachMessageRole.Assistant && message.Status == CoachGenerationStatus.Failed);
    }

    public DailyCoachBriefingResponse CreateDailyBriefing(DailyCoachBriefing briefing) => new(briefing.Id, briefing.LocalDate,
        briefing.Status, briefing.RequestedAt, briefing.ProcessingStartedAt, briefing.CompletedAt, briefing.FailedAt,
        briefing.LastFailureMessage, briefing.Status == CoachGenerationStatus.Failed, DeserializeDaily(briefing.ContentJson), WellnessDisclaimer);

    private static CoachAnswerContentResponse? DeserializeAnswer(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;
        try
        {
            var content = JsonSerializer.Deserialize<CoachAnswerStructuredOutput>(json, SerializerOptions);
            return content is null ? null : new CoachAnswerContentResponse(content.AnswerMarkdown,
                content.SuggestedActions.Select(MapAction).ToList(), content.DataLimitations, content.SafetyCategory, WellnessDisclaimer);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static DailyCoachBriefingContentResponse? DeserializeDaily(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;
        try
        {
            var content = JsonSerializer.Deserialize<DailyCoachBriefingStructuredOutput>(json, SerializerOptions);
            return content is null ? null : new DailyCoachBriefingContentResponse(content.Headline, content.Focus,
                content.SummaryMarkdown, MapAction(content.NextAction), content.InsightMarkdown, content.DataLimitations);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static CoachSuggestedActionResponse MapAction(CoachSuggestedAction action) =>
        new(action.Title, action.Description, action.Category);
}
