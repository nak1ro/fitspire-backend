namespace backend.Modules.Notification.Domain.Enums;

public enum NotificationType
{
    Follow = 1,
    PostLike = 2,
    PostComment = 3,
    GoalCompleted = 4,
    FollowRequest = 5,
    FollowRequestAccepted = 6,
    CommentLike = 7,
    CommentReply = 8,
    GoalPeriodFailed = 9,
    ChallengeInvitation = 10,
    ChallengeStarted = 11,
    ChallengeCancelled = 12,
    ChallengeCompleted = 13,
    ChallengeWon = 14,
    BadgeEarned = 15,
    ChallengeUpdated = 16
}
