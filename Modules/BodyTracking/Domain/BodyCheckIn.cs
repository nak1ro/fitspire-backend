using backend.Modules.BodyTracking.Domain.Constants;
using backend.Modules.Media.Domain;
using backend.Modules.Shared.Domain;
using backend.Modules.User.Domain;

namespace backend.Modules.BodyTracking.Domain;

public class BodyCheckIn : AggregateRoot<Guid>
{
    public Guid UserId { get; private set; }
    public DateOnly CheckInDate { get; private set; }
    public double? WeightKg { get; private set; }
    public double? BodyFatPercent { get; private set; }
    public double? WaistCm { get; private set; }
    public double? ChestCm { get; private set; }
    public double? HipsCm { get; private set; }
    public double? ArmCm { get; private set; }
    public double? ThighCm { get; private set; }
    public int? WellbeingScore { get; private set; }
    public string? Note { get; private set; }
    public Guid? PhotoMediaId { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    public AppUser User { get; private set; } = null!;
    public MediaAsset? PhotoMedia { get; private set; }

    private BodyCheckIn()
    {
    }

    public static BodyCheckIn Create(
        Guid id,
        Guid userId,
        DateOnly checkInDate,
        double? weightKg,
        double? bodyFatPercent,
        double? waistCm,
        double? chestCm,
        double? hipsCm,
        double? armCm,
        double? thighCm,
        int? wellbeingScore,
        string? note,
        Guid? photoMediaId)
    {
        if (id == Guid.Empty || userId == Guid.Empty)
            throw new DomainException("Body check-in identity and owner are required.");

        EnsureValidDate(checkInDate);
        var checkIn = new BodyCheckIn { Id = id, UserId = userId, CheckInDate = checkInDate, CreatedAt = DateTime.UtcNow };
        checkIn.Update(weightKg, bodyFatPercent, waistCm, chestCm, hipsCm, armCm, thighCm, wellbeingScore, note, photoMediaId);
        return checkIn;
    }

    public void Update(
        double? weightKg,
        double? bodyFatPercent,
        double? waistCm,
        double? chestCm,
        double? hipsCm,
        double? armCm,
        double? thighCm,
        int? wellbeingScore,
        string? note,
        Guid? photoMediaId)
    {
        EnsureActive();
        ValidateMeasurements(weightKg, bodyFatPercent, waistCm, chestCm, hipsCm, armCm, thighCm, wellbeingScore);
        var normalizedNote = NormalizeNote(note);
        EnsureMeaningfulContent(weightKg, bodyFatPercent, waistCm, chestCm, hipsCm, armCm, thighCm, wellbeingScore, normalizedNote, photoMediaId);

        WeightKg = weightKg;
        BodyFatPercent = bodyFatPercent;
        WaistCm = waistCm;
        ChestCm = chestCm;
        HipsCm = hipsCm;
        ArmCm = armCm;
        ThighCm = thighCm;
        WellbeingScore = wellbeingScore;
        Note = normalizedNote;
        PhotoMediaId = photoMediaId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeDate(DateOnly checkInDate)
    {
        EnsureActive();
        EnsureValidDate(checkInDate);
        CheckInDate = checkInDate;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        EnsureActive();
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DeletedAt;
    }

    private void EnsureActive()
    {
        if (DeletedAt is not null)
            throw new DomainException("A deleted body check-in cannot be changed.");
    }

    private static void ValidateMeasurements(
        double? weightKg,
        double? bodyFatPercent,
        double? waistCm,
        double? chestCm,
        double? hipsCm,
        double? armCm,
        double? thighCm,
        int? wellbeingScore)
    {
        ValidateRange(weightKg, BodyCheckInLimits.MaximumWeightKg, "Weight");
        ValidateRange(bodyFatPercent, BodyCheckInLimits.MaximumBodyFatPercent, "Body fat percentage", allowZero: true);
        ValidateRange(waistCm, BodyCheckInLimits.MaximumCircumferenceCm, "Waist circumference");
        ValidateRange(chestCm, BodyCheckInLimits.MaximumCircumferenceCm, "Chest circumference");
        ValidateRange(hipsCm, BodyCheckInLimits.MaximumCircumferenceCm, "Hip circumference");
        ValidateRange(armCm, BodyCheckInLimits.MaximumCircumferenceCm, "Arm circumference");
        ValidateRange(thighCm, BodyCheckInLimits.MaximumCircumferenceCm, "Thigh circumference");

        if (wellbeingScore is < BodyCheckInLimits.MinimumWellbeingScore or > BodyCheckInLimits.MaximumWellbeingScore)
            throw new DomainException("Wellbeing score must be between 1 and 5.");
    }

    private static void ValidateRange(double? value, double maximum, string name, bool allowZero = false)
    {
        if (!value.HasValue)
            return;

        if (double.IsNaN(value.Value) || double.IsInfinity(value.Value) || value.Value < 0 || (!allowZero && value.Value == 0) || value.Value > maximum)
            throw new DomainException($"{name} must be {(allowZero ? "at least" : "greater than")} zero and no more than {maximum}.");
    }

    private static string? NormalizeNote(string? note)
    {
        if (string.IsNullOrWhiteSpace(note))
            return null;

        var normalized = note.Trim();
        if (normalized.Length > BodyCheckInLimits.MaximumNoteLength)
            throw new DomainException($"Body check-in note must be at most {BodyCheckInLimits.MaximumNoteLength} characters.");

        return normalized;
    }

    private static void EnsureMeaningfulContent(
        double? weightKg,
        double? bodyFatPercent,
        double? waistCm,
        double? chestCm,
        double? hipsCm,
        double? armCm,
        double? thighCm,
        int? wellbeingScore,
        string? note,
        Guid? photoMediaId)
    {
        if (weightKg.HasValue || bodyFatPercent.HasValue || waistCm.HasValue || chestCm.HasValue || hipsCm.HasValue ||
            armCm.HasValue || thighCm.HasValue || wellbeingScore.HasValue || note is not null || photoMediaId.HasValue)
            return;

        throw new DomainException("A body check-in must contain at least one measurement, wellbeing score, note, or photo.");
    }

    private static void EnsureValidDate(DateOnly checkInDate)
    {
        if (checkInDate == DateOnly.MinValue)
            throw new DomainException("Body check-in date is required.");
    }
}
