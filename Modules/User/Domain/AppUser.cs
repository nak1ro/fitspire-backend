using System.Text.RegularExpressions;
using backend.Modules.Badge.Domain;
using backend.Modules.AiCoaching.Domain;
using backend.Modules.BodyTracking.Domain;
using backend.Modules.Challenge.Domain;
using backend.Modules.Goal.Domain.Entities;
using backend.Modules.Media.Domain;
using backend.Modules.Notification.Domain;
using backend.Modules.Nutrition.Domain;
using backend.Modules.Social.Domain;
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
}
