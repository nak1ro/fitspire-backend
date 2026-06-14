namespace backend.Modules.Social.Contracts.Posts;

public record CreatePostRequest(string Content, string? ImageUrl = null);
public record UpdatePostRequest(string Content, string? ImageUrl = null);
public record ShareWorkoutRequest(string? Caption = null);
public record CommentRequest(string Content, Guid? ReplyToCommentId = null);
public record LikeResponse(bool IsLiked);
public record FollowResponse(bool IsFollowing, bool IsRequestPending = false);
