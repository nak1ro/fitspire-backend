using backend.Modules.Badge.Domain;
using backend.Modules.BodyTracking.Domain;
using backend.Modules.Challenge.Domain;
using backend.Modules.Goal.Domain.Entities;
using backend.Modules.Group.Domain;
using backend.Modules.Moderation.Domain;
using backend.Modules.Media.Domain;
using backend.Modules.Notification.Domain;
using backend.Modules.Nutrition.Domain;
using backend.Modules.Progress.Domain;
using backend.Modules.Social.Domain;
using backend.Modules.User.Domain;
using backend.Modules.Workout.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace backend.Data;

public class FitspireDbContext : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>
{
    public FitspireDbContext(DbContextOptions<FitspireDbContext> options)
        : base(options)
    {
    }

    public new DbSet<AppUser> Users { get; set; }
    public DbSet<AppUserPreference> UserPreferences { get; set; }
    public DbSet<MediaAsset> MediaAssets { get; set; }
    public DbSet<MediaVariant> MediaVariants { get; set; }
    public DbSet<BodyCheckIn> BodyCheckIns { get; set; }

    public DbSet<Post> Posts { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<PostLike> PostLikes { get; set; }
    public DbSet<CommentLike> CommentLikes { get; set; }
    public DbSet<PostMedia> PostMedia { get; set; }
    public DbSet<Follower> Followers { get; set; }
    public DbSet<FollowRequest> FollowRequests { get; set; }
    public DbSet<SavedPost> SavedPosts { get; set; }

    public DbSet<UserChallenge> Challenges { get; set; }
    public DbSet<ChallengeParticipant> ChallengeParticipants { get; set; }
    public DbSet<ChallengeInvitation> ChallengeInvitations { get; set; }
    public DbSet<ChallengeScoreContribution> ChallengeScoreContributions { get; set; }
    public DbSet<ChallengeResult> ChallengeResults { get; set; }

    public DbSet<UserGroup> Groups { get; set; }
    public DbSet<GroupMember> GroupMembers { get; set; }

    public DbSet<AppNotification> Notifications { get; set; }

    public DbSet<Report> Reports { get; set; }
    public DbSet<Ban> Bans { get; set; }

    // --- Nutrition ---
    public DbSet<Meal> Meals { get; set; }
    public DbSet<MealItem> MealItems { get; set; }

    public DbSet<UserGoal> Goals { get; set; }
    public DbSet<GoalType> GoalTypes { get; set; }
    public DbSet<GoalProgressEntry> GoalProgressEntries { get; set; }
    public DbSet<GoalPeriod> GoalPeriods { get; set; }
    public DbSet<GoalTargetChange> GoalTargetChanges { get; set; }

    public DbSet<AchievementBadge> Badges { get; set; }
    public DbSet<UserBadge> UserBadges { get; set; }

    public DbSet<MetricDefinition> MetricDefinitions { get; set; }
    public DbSet<ActivityContribution> ActivityContributions { get; set; }

    public DbSet<UserWorkout> UserWorkouts { get; set; }
    public DbSet<GymWorkoutExercise> GymWorkoutExercises { get; set; }
    public DbSet<GymWorkoutSet> GymWorkoutSets { get; set; }
    public DbSet<RunningUserWorkoutDetails> RunningUserWorkouts { get; set; }
    public DbSet<CyclingUserWorkoutDetails> CyclingUserWorkouts { get; set; }
    public DbSet<SwimmingUserWorkoutDetails> SwimmingUserWorkouts { get; set; }
    public DbSet<YogaUserWorkoutDetails> YogaUserWorkouts { get; set; }
    public DbSet<Exercise> Exercises { get; set; }
    public DbSet<ExerciseCategory> ExerciseCategories { get; set; }
    public DbSet<WorkoutRoutine> WorkoutRoutines { get; set; }
    public DbSet<PersonalRecord> PersonalRecords { get; set; }
    public DbSet<PersonalRecordHistory> PersonalRecordHistory { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FitspireDbContext).Assembly);
    }
}
