namespace backend.Modules.Social.Contracts.Posts;

public record CreatePostRequest(string? Content, IReadOnlyList<Guid>? MediaAssetIds = null);
public record UpdatePostRequest(string? Content = null, IReadOnlyList<Guid>? MediaAssetIds = null);
public record ShareWorkoutRequest(Guid WorkoutId, string? Caption = null, IReadOnlyList<Guid>? MediaAssetIds = null);
public record ShareGoalRequest(Guid GoalId, string? Caption = null, IReadOnlyList<Guid>? MediaAssetIds = null);
public record CommentRequest(string Content, Guid? ReplyToCommentId = null);
public record LikeResponse(bool IsLiked);
public record FollowResponse(bool IsFollowing, bool IsRequestPending = false);
public record SavePostResponse(bool IsSaved);
