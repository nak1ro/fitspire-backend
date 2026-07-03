using backend.Modules.Media.Contracts;

namespace backend.Modules.Social.Contracts.Comments;

public record CommentResponse(
    Guid Id,
    Guid UserId,
    string UserName,
    string? UserAvatarUrl,
    MediaResponse? UserAvatar,
    string Content,
    Guid? RootCommentId,
    Guid? ReplyToCommentId,
    int LikesCount,
    bool IsLikedByCurrentUser,
    int RepliesCount,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record UpdateCommentRequest(string Content);
