using backend.Modules.Social.Features.Common;
using backend.Modules.Social.Features.Feed;
using backend.Modules.Social.Features.Follow;
using backend.Modules.Social.Features.Posts;
using backend.Modules.Shared.Extensions;
using backend.Modules.Shared.Domain;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Modules.Social;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SocialController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IValidator<CreatePostRequest> _createPostValidator;
    private readonly IValidator<UpdatePostRequest> _updatePostValidator;
    private readonly IValidator<CommentRequest> _commentValidator;

    public SocialController(
        IMediator mediator,
        IValidator<CreatePostRequest> createPostValidator,
        IValidator<UpdatePostRequest> updatePostValidator,
        IValidator<CommentRequest> commentValidator)
    {
        _mediator = mediator;
        _createPostValidator = createPostValidator;
        _updatePostValidator = updatePostValidator;
        _commentValidator = commentValidator;
    }

    /// <summary>
    /// Get the user's feed (posts from followed users).
    /// </summary>
    [HttpGet("feed")]
    public async Task<ActionResult<List<FeedItemResponse>>> GetFeed([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        ValidatePagination(page, pageSize);

        var userId = User.GetRequiredUserId();
        var feed = await _mediator.Send(new GetUserFeedQuery(userId, page, pageSize));
        return Ok(feed);
    }

    /// <summary>
    /// Get posts created by a specific user.
    /// </summary>
    [HttpGet("users/{targetUserId:guid}/posts")]
    public async Task<ActionResult<List<FeedItemResponse>>> GetUserPosts(
        Guid targetUserId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        ValidatePagination(page, pageSize);

        var viewerUserId = User.GetRequiredUserId();
        var posts = await _mediator.Send(new GetUserPostsQuery(viewerUserId, targetUserId, page, pageSize));
        return Ok(posts);
    }

    /// <summary>
    /// Create a new text post.
    /// </summary>
    [HttpPost("posts")]
    public async Task<ActionResult<Guid>> CreatePost([FromBody] CreatePostRequest request)
    {
        await _createPostValidator.ValidateAndThrowAsync(request);

        var userId = User.GetRequiredUserId();
        var postId = await _mediator.Send(new CreatePostCommand(userId, request.Content, request.ImageUrl));
        return CreatedAtAction(nameof(GetFeed), new { id = postId }, postId);
    }

    /// <summary>
    /// Update one of the current user's text posts.
    /// </summary>
    [HttpPatch("posts/{postId:guid}")]
    public async Task<IActionResult> UpdatePost(Guid postId, [FromBody] UpdatePostRequest request)
    {
        await _updatePostValidator.ValidateAndThrowAsync(request);

        var userId = User.GetRequiredUserId();
        await _mediator.Send(new UpdatePostCommand(userId, postId, request.Content, request.ImageUrl));
        return NoContent();
    }

    /// <summary>
    /// Delete one of the current user's posts.
    /// </summary>
    [HttpDelete("posts/{postId:guid}")]
    public async Task<IActionResult> DeletePost(Guid postId)
    {
        var userId = User.GetRequiredUserId();
        await _mediator.Send(new DeletePostCommand(userId, postId));
        return NoContent();
    }

    /// <summary>
    /// Like or unlike a post (toggle).
    /// </summary>
    [HttpPost("posts/{postId:guid}/like")]
    public async Task<ActionResult<LikeResponse>> LikePost(Guid postId)
    {
        var userId = User.GetRequiredUserId();
        var isLiked = await _mediator.Send(new LikePostCommand(userId, postId));
        return Ok(new LikeResponse(isLiked));
    }

    /// <summary>
    /// Comment on a post.
    /// </summary>
    [HttpPost("posts/{postId:guid}/comments")]
    public async Task<ActionResult<Guid>> CommentOnPost(Guid postId, [FromBody] CommentRequest request)
    {
        await _commentValidator.ValidateAndThrowAsync(request);

        var userId = User.GetRequiredUserId();
        var commentId = await _mediator.Send(new CommentOnPostCommand(userId, postId, request.Content));
        return Ok(commentId);
    }

    /// <summary>
    /// Delete a comment as the comment author or post owner.
    /// </summary>
    [HttpDelete("posts/{postId:guid}/comments/{commentId:guid}")]
    public async Task<IActionResult> DeleteComment(Guid postId, Guid commentId)
    {
        var userId = User.GetRequiredUserId();
        await _mediator.Send(new DeleteCommentCommand(userId, postId, commentId));
        return NoContent();
    }

    /// <summary>
    /// Follow or unfollow a user (toggle).
    /// </summary>
    [HttpPost("follow/{targetUserId:guid}")]
    public async Task<ActionResult<FollowResponse>> FollowUser(Guid targetUserId)
    {
        var userId = User.GetRequiredUserId();
        var isFollowing = await _mediator.Send(new FollowUserCommand(userId, targetUserId));
        return Ok(new FollowResponse(isFollowing));
    }

    private static void ValidatePagination(int page, int pageSize)
    {
        if (page < 1)
            throw new DomainException("Page must be greater than zero.");

        if (pageSize is < 1 or > 100)
            throw new DomainException("Page size must be between 1 and 100.");
    }
}

// DTOs
public record CreatePostRequest(string Content, string? ImageUrl = null);
public record UpdatePostRequest(string Content, string? ImageUrl = null);
public record CommentRequest(string Content);
public record LikeResponse(bool IsLiked);
public record FollowResponse(bool IsFollowing);
