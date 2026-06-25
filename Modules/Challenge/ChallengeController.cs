using backend.Modules.Challenge.Contracts;
using backend.Modules.Challenge.Features;
using backend.Modules.Shared.Extensions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Modules.Challenge;

[ApiController]
[Route("api/challenges")]
[Authorize]
public class ChallengeController : ControllerBase
{
    private readonly IMediator _mediator; private readonly IValidator<CreateChallengeRequest> _createValidator; private readonly IValidator<InviteChallengeUserRequest> _inviteValidator;
    public ChallengeController(IMediator mediator, IValidator<CreateChallengeRequest> createValidator, IValidator<InviteChallengeUserRequest> inviteValidator) { _mediator = mediator; _createValidator = createValidator; _inviteValidator = inviteValidator; }

    [HttpPost] public async Task<ActionResult<Guid>> Create(CreateChallengeRequest request) { await _createValidator.ValidateAndThrowAsync(request); var id = await _mediator.Send(new CreateChallengeCommand(User.GetRequiredUserId(), request)); return CreatedAtAction(nameof(Get), new { challengeId = id }, id); }
    [HttpGet("discover")] public async Task<ActionResult<List<ChallengeResponse>>> Discover([FromQuery] int page = 1, [FromQuery] int pageSize = 20) => Ok(await _mediator.Send(new DiscoverChallengesQuery(User.GetRequiredUserId(), page, pageSize)));
    [HttpGet("mine")] public async Task<ActionResult<List<ChallengeResponse>>> Mine([FromQuery] int page = 1, [FromQuery] int pageSize = 20) => Ok(await _mediator.Send(new GetMyChallengesQuery(User.GetRequiredUserId(), page, pageSize)));
    [HttpGet("{challengeId:guid}")] public async Task<ActionResult<ChallengeResponse>> Get(Guid challengeId) => Ok(await _mediator.Send(new GetChallengeQuery(User.GetRequiredUserId(), challengeId)));
    [HttpPost("{challengeId:guid}/join")] public async Task<IActionResult> Join(Guid challengeId) { await _mediator.Send(new JoinChallengeCommand(User.GetRequiredUserId(), challengeId)); return NoContent(); }
    [HttpDelete("{challengeId:guid}/participants/me")] public async Task<IActionResult> Leave(Guid challengeId) { await _mediator.Send(new LeaveChallengeCommand(User.GetRequiredUserId(), challengeId)); return NoContent(); }
    [HttpDelete("{challengeId:guid}/participants/{userId:guid}")] public async Task<IActionResult> RemoveParticipant(Guid challengeId, Guid userId) { await _mediator.Send(new RemoveChallengeParticipantCommand(User.GetRequiredUserId(), challengeId, userId)); return NoContent(); }
    [HttpPost("{challengeId:guid}/cancel")] public async Task<IActionResult> Cancel(Guid challengeId) { await _mediator.Send(new CancelChallengeCommand(User.GetRequiredUserId(), challengeId)); return NoContent(); }
    [HttpPost("{challengeId:guid}/invitations")] public async Task<IActionResult> Invite(Guid challengeId, InviteChallengeUserRequest request) { await _inviteValidator.ValidateAndThrowAsync(request); await _mediator.Send(new InviteChallengeUserCommand(User.GetRequiredUserId(), challengeId, request.UserId)); return NoContent(); }
    [HttpPost("invitations/{invitationId:guid}/accept")] public async Task<IActionResult> Accept(Guid invitationId) { await _mediator.Send(new RespondChallengeInvitationCommand(User.GetRequiredUserId(), invitationId, true)); return NoContent(); }
    [HttpPost("invitations/{invitationId:guid}/reject")] public async Task<IActionResult> Reject(Guid invitationId) { await _mediator.Send(new RespondChallengeInvitationCommand(User.GetRequiredUserId(), invitationId, false)); return NoContent(); }
    [HttpGet("{challengeId:guid}/leaderboard")] public async Task<ActionResult<List<ChallengeLeaderboardEntry>>> Leaderboard(Guid challengeId) => Ok(await _mediator.Send(new GetChallengeLeaderboardQuery(User.GetRequiredUserId(), challengeId)));
    [HttpGet("{challengeId:guid}/results")] public async Task<ActionResult<List<ChallengeLeaderboardEntry>>> Results(Guid challengeId) => Ok(await _mediator.Send(new GetChallengeResultsQuery(User.GetRequiredUserId(), challengeId)));
}
