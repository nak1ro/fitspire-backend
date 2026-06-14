namespace backend.Modules.Social.Contracts.Comments;

public record CommentResponse(
    Guid Id,
    Guid UserId,
    string UserName,
    string? UserAvatarUrl,
    string Content,
    Guid? RootCommentId,
    Guid? ReplyToCommentId,
    int LikesCount,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record UpdateCommentRequest(string Content);
