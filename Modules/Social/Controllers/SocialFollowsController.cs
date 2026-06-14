using backend.Modules.Shared.Extensions;
using backend.Modules.Social.Contracts.Posts;
using backend.Modules.Social.Contracts.Follows;
using backend.Modules.Social.Features.Follow;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Modules.Social.Controllers;

[ApiController]
[Authorize]
[Route("api/social")]
public class SocialFollowsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SocialFollowsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("users/{userId:guid}/follow")]
    public async Task<ActionResult<FollowResponse>> Follow(Guid userId)
    {
        var response = await _mediator.Send(new FollowUserCommand(User.GetRequiredUserId(), userId));
        return Ok(response);
    }

    [HttpDelete("users/{userId:guid}/follow")]
    public async Task<IActionResult> Unfollow(Guid userId)
    {
        await _mediator.Send(new UnfollowUserCommand(User.GetRequiredUserId(), userId));
        return NoContent();
    }

    [HttpDelete("users/{userId:guid}/followers")]
    public async Task<IActionResult> RemoveFollower(Guid userId)
    {
        await _mediator.Send(new RemoveFollowerCommand(User.GetRequiredUserId(), userId));
        return NoContent();
    }

    [HttpGet("follow-requests/incoming")]
    public async Task<ActionResult<List<FollowRequestResponse>>> GetIncomingRequests(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var requests = await _mediator.Send(new GetFollowRequestsQuery(User.GetRequiredUserId(), true, page, pageSize));
        return Ok(requests);
    }

    [HttpGet("follow-requests/outgoing")]
    public async Task<ActionResult<List<FollowRequestResponse>>> GetOutgoingRequests(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var requests = await _mediator.Send(new GetFollowRequestsQuery(User.GetRequiredUserId(), false, page, pageSize));
        return Ok(requests);
    }

    [HttpDelete("follow-requests/{requestId:guid}")]
    public async Task<IActionResult> CancelRequest(Guid requestId)
    {
        await _mediator.Send(new CancelFollowRequestCommand(User.GetRequiredUserId(), requestId));
        return NoContent();
    }

    [HttpPost("follow-requests/{requestId:guid}/accept")]
    public async Task<IActionResult> AcceptRequest(Guid requestId)
    {
        await _mediator.Send(new DecideFollowRequestCommand(User.GetRequiredUserId(), requestId, true));
        return NoContent();
    }

    [HttpPost("follow-requests/{requestId:guid}/reject")]
    public async Task<IActionResult> RejectRequest(Guid requestId)
    {
        await _mediator.Send(new DecideFollowRequestCommand(User.GetRequiredUserId(), requestId, false));
        return NoContent();
    }
}
