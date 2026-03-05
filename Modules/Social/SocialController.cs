using backend.Modules.Social.Features.Feed;
using backend.Modules.Social.Features.Follow;
using backend.Modules.Social.Features.Posts;
using backend.Modules.Shared.Extensions;
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

    public SocialController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get the user's feed (posts from followed users).
    /// </summary>
    [HttpGet("feed")]
    public async Task<ActionResult<List<FeedItemResponse>>> GetFeed([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = User.GetRequiredUserId();
        var feed = await _mediator.Send(new GetUserFeedQuery(userId, page, pageSize));
        return Ok(feed);
    }

    /// <summary>
    /// Create a new text post.
    /// </summary>
    [HttpPost("posts")]
    public async Task<ActionResult<Guid>> CreatePost([FromBody] CreatePostRequest request)
    {
        var userId = User.GetRequiredUserId();
        var postId = await _mediator.Send(new CreatePostCommand(userId, request.Content, request.ImageUrl));
        return CreatedAtAction(nameof(GetFeed), new { id = postId }, postId);
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
        var userId = User.GetRequiredUserId();
        var commentId = await _mediator.Send(new CommentOnPostCommand(userId, postId, request.Content));
        return Ok(commentId);
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
}

// DTOs
public record CreatePostRequest(string Content, string? ImageUrl = null);
public record CommentRequest(string Content);
public record LikeResponse(bool IsLiked);
public record FollowResponse(bool IsFollowing);
