using backend.Modules.Badge.Contracts;
using backend.Modules.Social.Features.Common;
using backend.Modules.Social.Contracts.Posts;
using backend.Modules.Social.Contracts.Comments;
using backend.Modules.Social.Features.Feed;
using backend.Modules.Social.Features.Follow;
using backend.Modules.Social.Features.Posts;
using backend.Modules.Shared.Extensions;
using backend.Modules.Shared.Domain;
using backend.Modules.Goal.DTOs;
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
    private readonly IValidator<UpdateCommentRequest> _updateCommentValidator;
    private readonly IValidator<ShareWorkoutRequest> _shareWorkoutValidator;
    private readonly IValidator<ShareGoalRequest> _shareGoalValidator;
    private readonly IValidator<PublicBadgeFilter> _publicBadgeFilterValidator;

    public SocialController(
        IMediator mediator,
        IValidator<CreatePostRequest> createPostValidator,
        IValidator<UpdatePostRequest> updatePostValidator,
        IValidator<CommentRequest> commentValidator,
        IValidator<UpdateCommentRequest> updateCommentValidator,
        IValidator<ShareWorkoutRequest> shareWorkoutValidator,
        IValidator<ShareGoalRequest> shareGoalValidator,
        IValidator<PublicBadgeFilter> publicBadgeFilterValidator)
    {
        _mediator = mediator;
        _createPostValidator = createPostValidator;
        _updatePostValidator = updatePostValidator;
        _commentValidator = commentValidator;
        _updateCommentValidator = updateCommentValidator;
        _shareWorkoutValidator = shareWorkoutValidator;
        _shareGoalValidator = shareGoalValidator;
        _publicBadgeFilterValidator = publicBadgeFilterValidator;
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

    [HttpGet("discover")]
    public async Task<ActionResult<List<FeedItemResponse>>> GetDiscoverFeed([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        ValidatePagination(page, pageSize);
        var feed = await _mediator.Send(new GetDiscoverFeedQuery(User.GetRequiredUserId(), page, pageSize));
        return Ok(feed);
    }

    [HttpGet("posts/{postId:guid}")]
    public async Task<ActionResult<FeedItemResponse>> GetPost(Guid postId)
    {
        var post = await _mediator.Send(new GetPostDetailQuery(User.GetRequiredUserId(), postId));
        return Ok(post);
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

    [HttpGet("users/{targetUserId:guid}/goals")]
    public async Task<ActionResult<List<Contracts.Profiles.PublicGoalResponse>>> GetUserGoals(Guid targetUserId) =>
        Ok(await _mediator.Send(new Features.Profiles.GetPublicGoalsQuery(User.GetRequiredUserId(), targetUserId)));

    [HttpGet("users/{targetUserId:guid}/goal-periods")]
    public async Task<ActionResult<GoalPageResponse<Contracts.Profiles.PublicGoalPeriodResponse>>> GetUserGoalPeriods(
        Guid targetUserId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        ValidatePagination(page, pageSize);
        return Ok(await _mediator.Send(new Features.Profiles.GetPublicGoalPeriodsQuery(
            User.GetRequiredUserId(), targetUserId, page, pageSize)));
    }

    [HttpGet("users/{targetUserId:guid}/badges")]
    public async Task<ActionResult<BadgePageResponse<PublicBadgeResponse>>> GetUserBadges(Guid targetUserId,
        [FromQuery] PublicBadgeFilter filter)
    {
        await _publicBadgeFilterValidator.ValidateAndThrowAsync(filter);
        return Ok(await _mediator.Send(new Features.Profiles.GetPublicBadgesQuery(User.GetRequiredUserId(), targetUserId, filter)));
    }

    [HttpGet("users/{targetUserId:guid}/badges/featured")]
    public async Task<ActionResult<IReadOnlyList<PublicBadgeResponse>>> GetFeaturedUserBadges(Guid targetUserId) =>
        Ok(await _mediator.Send(new Features.Profiles.GetFeaturedPublicBadgesQuery(User.GetRequiredUserId(), targetUserId)));

    [HttpGet("users/{targetUserId:guid}/challenge-results")]
    public async Task<ActionResult<List<Contracts.Profiles.PublicChallengeResultResponse>>> GetUserChallengeResults(Guid targetUserId) =>
        Ok(await _mediator.Send(new Features.Profiles.GetPublicChallengeResultsQuery(User.GetRequiredUserId(), targetUserId)));

    /// <summary>
    /// Create a new text post.
    /// </summary>
    [HttpPost("posts")]
    public async Task<ActionResult<Guid>> CreatePost([FromBody] CreatePostRequest request)
    {
        await _createPostValidator.ValidateAndThrowAsync(request);

        var userId = User.GetRequiredUserId();
        var postId = await _mediator.Send(new CreatePostCommand(userId, request.Content, request.MediaAssetIds));
        return CreatedAtAction(nameof(GetPost), new { postId }, postId);
    }

    /// <summary>
    /// Update one of the current user's text posts.
    /// </summary>
    [HttpPatch("posts/{postId:guid}")]
    public async Task<IActionResult> UpdatePost(Guid postId, [FromBody] UpdatePostRequest request)
    {
        await _updatePostValidator.ValidateAndThrowAsync(request);

        var userId = User.GetRequiredUserId();
        await _mediator.Send(new UpdatePostCommand(userId, postId, request.Content, request.MediaAssetIds));
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

    [HttpPost("workout-shares")]
    public async Task<ActionResult<Guid>> ShareWorkout([FromBody] ShareWorkoutRequest request)
    {
        await _shareWorkoutValidator.ValidateAndThrowAsync(request);
        var postId = await _mediator.Send(new ShareWorkoutCommand(User.GetRequiredUserId(), request.WorkoutId, request.Caption, request.MediaAssetIds));
        return CreatedAtAction(nameof(GetPost), new { postId }, postId);
    }

    /// <summary>
    /// Get the IDs of workouts the current user has already shared, so a picker UI can exclude them.
    /// </summary>
    [HttpGet("workout-shares/mine")]
    public async Task<ActionResult<List<Guid>>> GetMySharedWorkoutIds() =>
        Ok(await _mediator.Send(new GetSharedWorkoutIdsQuery(User.GetRequiredUserId())));

    [HttpPost("goal-shares")]
    public async Task<ActionResult<Guid>> ShareGoal([FromBody] ShareGoalRequest request)
    {
        await _shareGoalValidator.ValidateAndThrowAsync(request);
        var postId = await _mediator.Send(new ShareGoalCommand(User.GetRequiredUserId(), request.GoalId, request.Caption, request.MediaAssetIds));
        return CreatedAtAction(nameof(GetPost), new { postId }, postId);
    }

    /// <summary>
    /// Get the IDs of goals the current user has already shared, so a picker UI can exclude them.
    /// </summary>
    [HttpGet("goal-shares/mine")]
    public async Task<ActionResult<List<Guid>>> GetMySharedGoalIds() =>
        Ok(await _mediator.Send(new GetSharedGoalIdsQuery(User.GetRequiredUserId())));

    [HttpPost("posts/{postId:guid}/likes")]
    public async Task<ActionResult<LikeResponse>> AddPostLike(Guid postId)
    {
        var response = await _mediator.Send(new LikePostCommand(User.GetRequiredUserId(), postId, true));
        return Ok(response);
    }

    [HttpPost("posts/{postId:guid}/saved")]
    public async Task<ActionResult<SavePostResponse>> SavePost(Guid postId) =>
        Ok(await _mediator.Send(new SavePostCommand(User.GetRequiredUserId(), postId, true)));

    [HttpDelete("posts/{postId:guid}/saved")]
    public async Task<ActionResult<SavePostResponse>> UnsavePost(Guid postId) =>
        Ok(await _mediator.Send(new SavePostCommand(User.GetRequiredUserId(), postId, false)));

    [HttpGet("saved-posts")]
    public async Task<ActionResult<List<FeedItemResponse>>> GetSavedPosts([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        ValidatePagination(page, pageSize);
        return Ok(await _mediator.Send(new GetSavedPostsQuery(User.GetRequiredUserId(), page, pageSize)));
    }

    [HttpDelete("posts/{postId:guid}/likes")]
    public async Task<ActionResult<LikeResponse>> RemovePostLike(Guid postId)
    {
        var response = await _mediator.Send(new LikePostCommand(User.GetRequiredUserId(), postId, false));
        return Ok(response);
    }

    [HttpGet("posts/{postId:guid}/likes")]
    public async Task<ActionResult<List<Contracts.Profiles.SocialUserSummaryResponse>>> GetPostLikes(
        Guid postId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        ValidatePagination(page, pageSize);
        var users = await _mediator.Send(new GetPostLikedByQuery(User.GetRequiredUserId(), postId, page, pageSize));
        return Ok(users);
    }

    /// <summary>
    /// Comment on a post.
    /// </summary>
    [HttpPost("posts/{postId:guid}/comments")]
    public async Task<ActionResult<Guid>> CommentOnPost(Guid postId, [FromBody] CommentRequest request)
    {
        await _commentValidator.ValidateAndThrowAsync(request);

        var userId = User.GetRequiredUserId();
        var commentId = await _mediator.Send(new CommentOnPostCommand(userId, postId, request.Content, request.ReplyToCommentId));
        return Ok(commentId);
    }

    [HttpGet("posts/{postId:guid}/comments")]
    public async Task<ActionResult<List<CommentResponse>>> GetPostComments(
        Guid postId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        ValidatePagination(page, pageSize);
        var comments = await _mediator.Send(new GetPostCommentsQuery(User.GetRequiredUserId(), postId, page, pageSize));
        return Ok(comments);
    }

    [HttpGet("posts/{postId:guid}/comments/{commentId:guid}/replies")]
    public async Task<ActionResult<List<CommentResponse>>> GetCommentReplies(
        Guid postId,
        Guid commentId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        ValidatePagination(page, pageSize);
        var replies = await _mediator.Send(new GetCommentRepliesQuery(User.GetRequiredUserId(), postId, commentId, page, pageSize));
        return Ok(replies);
    }

    [HttpPatch("posts/{postId:guid}/comments/{commentId:guid}")]
    public async Task<IActionResult> UpdateComment(Guid postId, Guid commentId, [FromBody] UpdateCommentRequest request)
    {
        await _updateCommentValidator.ValidateAndThrowAsync(request);
        await _mediator.Send(new UpdateCommentCommand(User.GetRequiredUserId(), postId, commentId, request.Content));
        return NoContent();
    }

    [HttpPost("posts/{postId:guid}/comments/{commentId:guid}/likes")]
    public async Task<ActionResult<LikeResponse>> AddCommentLike(Guid postId, Guid commentId)
    {
        var response = await _mediator.Send(new LikeCommentCommand(User.GetRequiredUserId(), postId, commentId, true));
        return Ok(response);
    }

    [HttpDelete("posts/{postId:guid}/comments/{commentId:guid}/likes")]
    public async Task<ActionResult<LikeResponse>> RemoveCommentLike(Guid postId, Guid commentId)
    {
        var response = await _mediator.Send(new LikeCommentCommand(User.GetRequiredUserId(), postId, commentId, false));
        return Ok(response);
    }

    [HttpGet("posts/{postId:guid}/comments/{commentId:guid}/likes")]
    public async Task<ActionResult<List<Contracts.Profiles.SocialUserSummaryResponse>>> GetCommentLikes(
        Guid postId,
        Guid commentId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        ValidatePagination(page, pageSize);
        var users = await _mediator.Send(new GetCommentLikedByQuery(User.GetRequiredUserId(), postId, commentId, page, pageSize));
        return Ok(users);
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

    private static void ValidatePagination(int page, int pageSize)
    {
        if (page < 1)
            throw new DomainException("Page must be greater than zero.");

        if (pageSize is < 1 or > 100)
            throw new DomainException("Page size must be between 1 and 100.");
    }
}
