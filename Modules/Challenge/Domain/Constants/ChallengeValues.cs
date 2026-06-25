namespace backend.Modules.Challenge.Domain.Constants;

public static class ChallengeStatuses
{
    public const string Upcoming = "Upcoming";
    public const string Active = "Active";
    public const string Finalizing = "Finalizing";
    public const string Completed = "Completed";
    public const string Cancelled = "Cancelled";
}

public static class ChallengeModes
{
    public const string Target = "Target";
    public const string Leaderboard = "Leaderboard";
}

public static class ChallengeVisibilities
{
    public const string Public = "Public";
    public const string FollowersOnly = "FollowersOnly";
    public const string InviteOnly = "InviteOnly";
}

public static class ChallengeJoinClosingModes
{
    public const string AtStart = "AtStart";
    public const string AtEnd = "AtEnd";
}

public static class ChallengeParticipantStatuses
{
    public const string Active = "Active";
    public const string Left = "Left";
    public const string Removed = "Removed";
}

public static class ChallengeInvitationStatuses
{
    public const string Pending = "Pending";
    public const string Accepted = "Accepted";
    public const string Rejected = "Rejected";
    public const string Cancelled = "Cancelled";
    public const string Expired = "Expired";
}
