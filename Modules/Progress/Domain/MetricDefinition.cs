using backend.Modules.Shared.Domain;

namespace backend.Modules.Progress.Domain;

public class MetricDefinition : Entity<string>
{
    public string DisplayName { get; private set; } = null!;
    public string CanonicalUnit { get; private set; } = null!;
    public string Aggregation { get; private set; } = null!;
    public bool IsGoalSupported { get; private set; }
    public bool IsChallengeSupported { get; private set; }
    public bool IsBadgeSupported { get; private set; }
    public bool IsAnalyticsSupported { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; }

    private MetricDefinition() { }

    public MetricDefinition(string code, string displayName, string canonicalUnit, string aggregation, int displayOrder)
    {
        Id = code;
        DisplayName = displayName;
        CanonicalUnit = canonicalUnit;
        Aggregation = aggregation;
        DisplayOrder = displayOrder;
        IsGoalSupported = true;
        IsChallengeSupported = true;
        IsBadgeSupported = true;
        IsAnalyticsSupported = true;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }
}
