namespace backend.Modules.BodyTracking.Contracts;

[System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
public enum BodyCheckInPhotoOperation
{
    Keep = 1,
    Replace = 2,
    Remove = 3
}

public interface IBodyCheckInInput
{
    double? WeightKg { get; }
    double? BodyFatPercent { get; }
    double? WaistCm { get; }
    double? ChestCm { get; }
    double? HipsCm { get; }
    double? ArmCm { get; }
    double? ThighCm { get; }
    int? WellbeingScore { get; }
    string? Note { get; }
}

public record CreateBodyCheckInRequest(
    DateOnly CheckInDate,
    double? WeightKg,
    double? BodyFatPercent,
    double? WaistCm,
    double? ChestCm,
    double? HipsCm,
    double? ArmCm,
    double? ThighCm,
    int? WellbeingScore,
    string? Note,
    Guid? PhotoMediaId = null) : IBodyCheckInInput;

public record UpdateBodyCheckInRequest(
    DateOnly CheckInDate,
    double? WeightKg,
    double? BodyFatPercent,
    double? WaistCm,
    double? ChestCm,
    double? HipsCm,
    double? ArmCm,
    double? ThighCm,
    int? WellbeingScore,
    string? Note,
    BodyCheckInPhotoOperation PhotoOperation = BodyCheckInPhotoOperation.Keep,
    Guid? PhotoMediaId = null) : IBodyCheckInInput;

public record BodyCheckInResponse(
    Guid Id,
    DateOnly CheckInDate,
    double? WeightKg,
    double? BodyFatPercent,
    double? WaistCm,
    double? ChestCm,
    double? HipsCm,
    double? ArmCm,
    double? ThighCm,
    int? WellbeingScore,
    string? Note,
    Guid? PhotoMediaId,
    backend.Modules.Media.Contracts.MediaResponse? Photo,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record BodyCheckInHistoryFilter(DateOnly? From = null, DateOnly? To = null, int Page = 1, int PageSize = 20);

public record BodyCheckInPageResponse(IReadOnlyList<BodyCheckInResponse> Items, int Page, int PageSize, int TotalCount);

public record BodyCheckInSummaryFilter(DateOnly? From = null, DateOnly? To = null);

public record BodyMeasurementSnapshotResponse(
    double? WeightKg,
    double? BodyFatPercent,
    double? WaistCm,
    double? ChestCm,
    double? HipsCm,
    double? ArmCm,
    double? ThighCm);

public record BodyMeasurementChangeResponse(
    double? WeightKg,
    double? BodyFatPercent,
    double? WaistCm,
    double? ChestCm,
    double? HipsCm,
    double? ArmCm,
    double? ThighCm);

public record BodyCheckInChartPoint(
    DateOnly CheckInDate,
    double? WeightKg,
    double? BodyFatPercent,
    double? WaistCm,
    double? ChestCm,
    double? HipsCm,
    double? ArmCm,
    double? ThighCm,
    int? WellbeingScore);

public record BodyCheckInSummaryResponse(
    DateOnly From,
    DateOnly To,
    int ActiveCheckInCount,
    BodyMeasurementSnapshotResponse Baseline,
    BodyMeasurementSnapshotResponse Current,
    BodyMeasurementChangeResponse Changes,
    int? LatestWellbeingScore,
    IReadOnlyList<BodyCheckInChartPoint> ChartPoints);
