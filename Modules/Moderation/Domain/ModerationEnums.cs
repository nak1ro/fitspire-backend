using System.Text.Json.Serialization;

namespace backend.Modules.Moderation.Domain;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ModerationReportTargetType
{
    Profile = 1,
    Post = 2,
    Comment = 3,
    Media = 4
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ModerationMediaContext
{
    ProfilePicture = 1,
    PostImage = 2
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ModerationReportReason
{
    Spam = 1,
    Harassment = 2,
    InappropriateContent = 3,
    Impersonation = 4,
    Other = 5
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ModerationReportStatus
{
    Open = 1,
    Resolved = 2
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ModerationResolutionOutcome
{
    Dismissed = 1,
    ActionTaken = 2
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ModerationActionType
{
    ReportDismissed = 1,
    ContentRemoved = 2,
    ContentRestored = 3,
    UserSuspended = 4,
    UserUnsuspended = 5
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AdminModerationResolutionAction
{
    Dismiss = 1,
    RemoveTarget = 2,
    SuspendUser = 3,
    RemoveTargetAndSuspendUser = 4
}
