using System.Text.Json;
using backend.Modules.AiCoaching.Contracts;
using backend.Modules.AiCoaching.Domain;

namespace backend.Modules.AiCoaching.Services;

public interface IWeeklyCoachReportResponseFactory
{
    WeeklyCoachReportResponse Create(WeeklyCoachReport report);
}

public sealed class WeeklyCoachReportResponseFactory : IWeeklyCoachReportResponseFactory
{
    private const string WellnessDisclaimer = "This report provides general fitness and wellness guidance, not medical advice.";
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    public WeeklyCoachReportResponse Create(WeeklyCoachReport report)
    {
        var coverage = ReadCoverage(report.SnapshotJson);
        var content = string.IsNullOrWhiteSpace(report.ReportJson) ? null : ReadContent(report.ReportJson);
        return new WeeklyCoachReportResponse(report.Id, report.PeriodStart, report.PeriodEnd, report.Status,
            report.HasDisplayableContent, report.Status == WeeklyCoachReportStatus.Failed, report.GenerationCount,
            report.RequestedAt, report.ProcessingStartedAt, report.CompletedAt, report.FailedAt, report.LastFailureMessage,
            coverage, content, WellnessDisclaimer);
    }

    private static WeeklyCoachCoverageResponse ReadCoverage(string snapshotJson)
    {
        var snapshot = Read<WeeklyCoachSnapshot>(snapshotJson, "snapshot");
        return new WeeklyCoachCoverageResponse(Map(snapshot.Coverage.Workouts), Map(snapshot.Coverage.Goals),
            Map(snapshot.Coverage.Challenges), Map(snapshot.Coverage.Body), Map(snapshot.Coverage.Nutrition));
    }

    private static WeeklyCoachReportContentResponse ReadContent(string reportJson)
    {
        var report = Read<WeeklyCoachStructuredReport>(reportJson, "report");
        return new WeeklyCoachReportContentResponse(report.Headline, report.Overview,
            report.Wins.Select(Map).ToList(), report.Patterns.Select(Map).ToList(), report.NextWeekActions.Select(Map).ToList(),
            report.DataLimitations);
    }

    private static T Read<T>(string json, string name)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(json, SerializerOptions)
                   ?? throw new AiServiceUnavailableException($"Stored coaching {name} is temporarily unavailable.");
        }
        catch (JsonException exception)
        {
            throw new AiServiceUnavailableException($"Stored coaching {name} is temporarily unavailable.", exception);
        }
    }

    private static WeeklyCoachSectionCoverageResponse Map(WeeklyCoachSectionCoverage coverage) =>
        new(coverage.State.ToString(), coverage.RecordCount);

    private static WeeklyCoachObservationResponse Map(WeeklyCoachObservation observation) =>
        new(observation.Title, observation.Explanation, observation.Category, observation.EvidenceKeys);

    private static WeeklyCoachActionResponse Map(WeeklyCoachAction action) =>
        new(action.Title, action.Explanation, action.Category, action.EvidenceKeys);
}
