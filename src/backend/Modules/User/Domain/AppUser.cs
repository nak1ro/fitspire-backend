using System.Text.RegularExpressions;
using backend.Modules.Badge.Domain;
using backend.Modules.Challenge.Domain;
using backend.Modules.Friendship.Domain;
using backend.Modules.Goal.Domain;
using backend.Modules.Group.Domain;
using backend.Modules.Moderation.Domain;
using backend.Modules.Notification.Domain;
using backend.Modules.Nutrition.Domain;
using backend.Modules.Social.Domain;
using backend.Modules.Workout.Domain;
using Microsoft.AspNetCore.Identity;

namespace backend.Modules.User.Domain;

public class AppUser : IdentityUser<Guid>
{
    public string DisplayName { get; set; } = null!;
    public string? Bio { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public bool IsPrivate { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Preferences
    public AppUserPreference? AppUserPreference { get; set; }

    // Workouts
    public ICollection<UserWorkout> Workouts { get; set; } = new List<UserWorkout>();

    // Notifications
    public ICollection<AppNotification> Notifications { get; set; } = new List<AppNotification>();

    // Social
    public ICollection<Post> Posts { get; set; } = new List<Post>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<SavedPost> SavedPosts { get; set; } = new List<SavedPost>();
    public ICollection<Like> Likes { get; set; } = new List<Like>();
    public ICollection<Follower> Followers { get; set; } = new List<Follower>(); // Who follows me
    public ICollection<Follower> Following { get; set; } = new List<Follower>(); // Who I follow
    public ICollection<FriendshipConnection> Friends { get; set; } = new List<FriendshipConnection>();
    public ICollection<FriendshipRequest> FriendshipSentRequests { get; set; } = new List<FriendshipRequest>();
    public ICollection<FriendshipRequest> FriendshipReceivedRequests { get; set; } = new List<FriendshipRequest>();

    // Reports and Bans
    public ICollection<Report> ReportsMade { get; set; } = new List<Report>();
    public ICollection<Report> ReportsReceived { get; set; } = new List<Report>();
    public ICollection<Ban> Bans { get; set; } = new List<Ban>();

    // Goals and Progress
    public ICollection<UserGoal> Goals { get; set; } = new List<UserGoal>();
    public ICollection<ProgressEntry> ProgressEntries { get; set; } = new List<ProgressEntry>();

    // Badges and Records
    public ICollection<UserBadge> Badges { get; set; } = new List<UserBadge>();
    public ICollection<PersonalRecord> PersonalRecords { get; set; } = new List<PersonalRecord>();
    public ICollection<PersonalRecordHistory> RecordHistory { get; set; } = new List<PersonalRecordHistory>();

    // Nutrition
    public ICollection<Meal> Meals { get; set; } = new List<Meal>();

    // Groups
    public ICollection<UserGroup> GroupsCreated { get; set; } = new List<UserGroup>();
    public ICollection<GroupMember> GroupMemberships { get; set; } = new List<GroupMember>();

    // Challenges
    public ICollection<UserChallenge> ChallengesCreated { get; set; } = new List<UserChallenge>();
    public ICollection<ChallengeParticipant> ChallengeParticipants { get; set; } = new List<ChallengeParticipant>();
}