using System.Text.RegularExpressions;
using backend.Modules.Badge.Domain;
using backend.Modules.AiCoaching.Domain;
using backend.Modules.BodyTracking.Domain;
using backend.Modules.Challenge.Domain;
using backend.Modules.Goal.Domain.Entities;
using backend.Modules.Media.Domain;
using backend.Modules.Moderation.Domain;
using backend.Modules.Notification.Domain;
using backend.Modules.Nutrition.Domain;
using backend.Modules.Shared.Domain;
using backend.Modules.Social.Domain;
using backend.Modules.User.Domain.Enums;
using backend.Modules.Workout.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace backend.Modules.User.Domain;

public class AppUser : IdentityUser<Guid>
{
    public string DisplayName { get; set; } = null!;
    public string? Bio { get; set; }
    public Guid? ProfilePictureMediaId { get; private set; }
    public MediaAsset? ProfilePictureMedia { get; private set; }
    public bool IsPrivate { get; set; } = false;
    public FitnessSport? FavoriteSport { get; set; }
    public FitnessLevel? FitnessLevel { get; set; }
    public double? HeightCm { get; set; }
    public DateTime? SuspendedAtUtc { get; private set; }
    public DateTime? SuspendedUntilUtc { get; private set; }
    public string? SuspensionReason { get; private set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Preferences
    public AppUserPreference? AppUserPreference { get; set; }
    public ICollection<MediaAsset> MediaAssets { get; set; } = new List<MediaAsset>();

    public Guid? SetProfilePictureMedia(Guid mediaAssetId)
    {
        if (mediaAssetId == Guid.Empty)
            throw new ArgumentException("Profile picture media is required.", nameof(mediaAssetId));

        var previousMediaId = ProfilePictureMediaId;
        ProfilePictureMediaId = mediaAssetId;
        UpdatedAt = DateTime.UtcNow;
        return previousMediaId;
    }

    public Guid? RemoveProfilePictureMedia()
    {
        var previousMediaId = ProfilePictureMediaId;
        ProfilePictureMediaId = null;
        UpdatedAt = DateTime.UtcNow;
        return previousMediaId;
    }

    public bool IsSuspended(DateTime utcNow)
    {
        EnsureUtc(utcNow, nameof(utcNow));
        return SuspendedUntilUtc is not null && SuspendedUntilUtc > utcNow;
    }

    public void Suspend(DateTime untilUtc, string? reason, DateTime utcNow)
    {
        EnsureUtc(untilUtc, nameof(untilUtc));
        EnsureUtc(utcNow, nameof(utcNow));
        if (untilUtc <= utcNow)
            throw new DomainException("Suspension end time must be in the future.");

        SuspendedAtUtc = utcNow;
        SuspendedUntilUtc = untilUtc;
        SuspensionReason = NormalizeSuspensionReason(reason);
        UpdatedAt = utcNow;
    }

    public void Unsuspend(DateTime utcNow)
    {
        EnsureUtc(utcNow, nameof(utcNow));
        if (SuspendedUntilUtc is null && SuspendedAtUtc is null && SuspensionReason is null)
            return;

        SuspendedAtUtc = null;
        SuspendedUntilUtc = null;
        SuspensionReason = null;
        UpdatedAt = utcNow;
    }

    // Workouts
    public ICollection<UserWorkout> Workouts { get; set; } = new List<UserWorkout>();

    // Notifications
    public ICollection<AppNotification> Notifications { get; set; } = new List<AppNotification>();

    // Social
    public ICollection<Post> Posts { get; set; } = new List<Post>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<SavedPost> SavedPosts { get; set; } = new List<SavedPost>();
    public ICollection<PostLike> PostLikes { get; set; } = new List<PostLike>();
    public ICollection<CommentLike> CommentLikes { get; set; } = new List<CommentLike>();
    public ICollection<Follower> Followers { get; set; } = new List<Follower>(); // Who follows me
    public ICollection<Follower> Following { get; set; } = new List<Follower>(); // Who I follow
    // Goals and Progress
    public ICollection<UserGoal> Goals { get; set; } = new List<UserGoal>();
    public ICollection<BodyCheckIn> BodyCheckIns { get; set; } = new List<BodyCheckIn>();
    public ICollection<WeeklyCoachReport> WeeklyCoachReports { get; set; } = new List<WeeklyCoachReport>();
    public ICollection<CoachThread> CoachThreads { get; set; } = new List<CoachThread>();
    public ICollection<CoachMessage> CoachMessages { get; set; } = new List<CoachMessage>();
    public ICollection<DailyCoachBriefing> DailyCoachBriefings { get; set; } = new List<DailyCoachBriefing>();

    // Badges and Records
    public ICollection<UserBadge> Badges { get; set; } = new List<UserBadge>();
    public ICollection<PersonalRecord> PersonalRecords { get; set; } = new List<PersonalRecord>();
    public ICollection<PersonalRecordHistory> RecordHistory { get; set; } = new List<PersonalRecordHistory>();

    // Nutrition
    public ICollection<Meal> Meals { get; set; } = new List<Meal>();
    public NutritionTarget? NutritionTarget { get; set; }
    public ICollection<FavouriteFood> FavouriteFoods { get; set; } = new List<FavouriteFood>();

    // Challenges
    public ICollection<UserChallenge> ChallengesCreated { get; set; } = new List<UserChallenge>();
    public ICollection<ChallengeParticipant> ChallengeParticipants { get; set; } = new List<ChallengeParticipant>();
    public ICollection<ModerationReport> ReportsSubmitted { get; set; } = new List<ModerationReport>();
    public ICollection<ModerationReport> ReportsReceived { get; set; } = new List<ModerationReport>();
    public ICollection<ModerationReport> ReportsResolved { get; set; } = new List<ModerationReport>();
    public ICollection<ModerationAction> ModerationActions { get; set; } = new List<ModerationAction>();

    private static string? NormalizeSuspensionReason(string? reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            return null;

        var normalized = reason.Trim();
        if (normalized.Length > ModerationLimits.MaximumSuspensionReasonLength)
            throw new DomainException($"Suspension reason must be at most {ModerationLimits.MaximumSuspensionReasonLength} characters.");

        return normalized;
    }

    private static void EnsureUtc(DateTime value, string name)
    {
        if (value.Kind != DateTimeKind.Utc)
            throw new DomainException($"{name} must be in UTC.");
    }
}
