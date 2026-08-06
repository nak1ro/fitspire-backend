using backend.Modules.Challenge.Contracts;
using backend.Modules.Challenge.Features;
using backend.Modules.Shared.Domain;
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
    private readonly IMediator _mediator;
    private readonly IValidator<CreateChallengeRequest> _createValidator;
    private readonly IValidator<UpdateChallengeRequest> _updateValidator;
    private readonly IValidator<UpdateActiveChallengeCopyRequest> _activeCopyValidator;
    private readonly IValidator<InviteChallengeUserRequest> _inviteValidator;
    private readonly IValidator<ChallengeListFilter> _listValidator;

    public ChallengeController(IMediator mediator, IValidator<CreateChallengeRequest> createValidator,
        IValidator<UpdateChallengeRequest> updateValidator, IValidator<UpdateActiveChallengeCopyRequest> activeCopyValidator,
        IValidator<InviteChallengeUserRequest> inviteValidator,
        IValidator<ChallengeListFilter> listValidator)
    {
        _mediator = mediator;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _activeCopyValidator = activeCopyValidator;
        _inviteValidator = inviteValidator;
        _listValidator = listValidator;
    }

    [HttpPatch("{challengeId:guid}/copy")]
    public async Task<IActionResult> UpdateActiveCopy(Guid challengeId, UpdateActiveChallengeCopyRequest request)
    {
        await _activeCopyValidator.ValidateAndThrowAsync(request);
        await _mediator.Send(new UpdateActiveChallengeCopyCommand(User.GetRequiredUserId(), challengeId, request));
        return NoContent();
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateChallengeRequest request)
    {
        await _createValidator.ValidateAndThrowAsync(request);
        var id = await _mediator.Send(new CreateChallengeCommand(User.GetRequiredUserId(), request));
        return CreatedAtAction(nameof(Get), new { challengeId = id }, id);
    }

    [HttpPatch("{challengeId:guid}")]
    public async Task<IActionResult> Update(Guid challengeId, UpdateChallengeRequest request)
    {
        await _updateValidator.ValidateAndThrowAsync(request);
        await _mediator.Send(new UpdateChallengeCommand(User.GetRequiredUserId(), challengeId, request));
        return NoContent();
    }

    [HttpGet("discover")]
    public async Task<ActionResult<ChallengePageResponse<ChallengeResponse>>> Discover([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        ValidatePagination(page, pageSize);
        return Ok(await _mediator.Send(new DiscoverChallengesQuery(User.GetRequiredUserId(), page, pageSize)));
    }

    [HttpGet("available")]
    public async Task<ActionResult<ChallengePageResponse<ChallengeResponse>>> Available([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        ValidatePagination(page, pageSize);
        return Ok(await _mediator.Send(new GetAvailableChallengesQuery(User.GetRequiredUserId(), page, pageSize)));
    }

    [HttpGet("mine")]
    public async Task<ActionResult<ChallengePageResponse<ChallengeResponse>>> Mine([FromQuery] ChallengeListFilter filter)
    {
        await _listValidator.ValidateAndThrowAsync(filter);
        return Ok(await _mediator.Send(new GetMyChallengesQuery(User.GetRequiredUserId(), filter)));
    }

    [HttpGet("invitations/incoming")]
    public async Task<ActionResult<ChallengePageResponse<ChallengeInvitationResponse>>> IncomingInvitations([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        ValidatePagination(page, pageSize);
        return Ok(await _mediator.Send(new GetIncomingChallengeInvitationsQuery(User.GetRequiredUserId(), page, pageSize)));
    }

    [HttpGet("{challengeId:guid}")]
    public async Task<ActionResult<ChallengeDetailResponse>> Get(Guid challengeId) =>
        Ok(await _mediator.Send(new GetChallengeQuery(User.GetRequiredUserId(), challengeId)));

    [HttpPost("{challengeId:guid}/join")]
    public async Task<IActionResult> Join(Guid challengeId)
    {
        await _mediator.Send(new JoinChallengeCommand(User.GetRequiredUserId(), challengeId));
        return NoContent();
    }

    [HttpDelete("{challengeId:guid}/participants/me")]
    public async Task<IActionResult> Leave(Guid challengeId)
    {
        await _mediator.Send(new LeaveChallengeCommand(User.GetRequiredUserId(), challengeId));
        return NoContent();
    }

    [HttpDelete("{challengeId:guid}/participants/{userId:guid}")]
    public async Task<IActionResult> RemoveParticipant(Guid challengeId, Guid userId)
    {
        await _mediator.Send(new RemoveChallengeParticipantCommand(User.GetRequiredUserId(), challengeId, userId));
        return NoContent();
    }

    [HttpPost("{challengeId:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid challengeId)
    {
        await _mediator.Send(new CancelChallengeCommand(User.GetRequiredUserId(), challengeId));
        return NoContent();
    }

    [HttpPost("{challengeId:guid}/invitations")]
    public async Task<IActionResult> Invite(Guid challengeId, InviteChallengeUserRequest request)
    {
        await _inviteValidator.ValidateAndThrowAsync(request);
        await _mediator.Send(new InviteChallengeUserCommand(User.GetRequiredUserId(), challengeId, request.UserId));
        return NoContent();
    }

    [HttpGet("{challengeId:guid}/invitations")]
    public async Task<ActionResult<ChallengePageResponse<SentChallengeInvitationResponse>>> SentInvitations(Guid challengeId,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        ValidatePagination(page, pageSize);
        return Ok(await _mediator.Send(new GetSentChallengeInvitationsQuery(User.GetRequiredUserId(), challengeId, page, pageSize)));
    }

    [HttpDelete("invitations/{invitationId:guid}")]
    public async Task<IActionResult> CancelInvitation(Guid invitationId)
    {
        await _mediator.Send(new CancelChallengeInvitationCommand(User.GetRequiredUserId(), invitationId));
        return NoContent();
    }

    [HttpPost("invitations/{invitationId:guid}/accept")]
    public async Task<IActionResult> Accept(Guid invitationId)
    {
        await _mediator.Send(new RespondChallengeInvitationCommand(User.GetRequiredUserId(), invitationId, true));
        return NoContent();
    }

    [HttpPost("invitations/{invitationId:guid}/reject")]
    public async Task<IActionResult> Reject(Guid invitationId)
    {
        await _mediator.Send(new RespondChallengeInvitationCommand(User.GetRequiredUserId(), invitationId, false));
        return NoContent();
    }

    [HttpGet("{challengeId:guid}/leaderboard")]
    public async Task<ActionResult<ChallengePageResponse<ChallengeLeaderboardEntry>>> Leaderboard(Guid challengeId,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        ValidatePagination(page, pageSize);
        return Ok(await _mediator.Send(new GetChallengeLeaderboardQuery(User.GetRequiredUserId(), challengeId, page, pageSize)));
    }

    [HttpGet("{challengeId:guid}/results")]
    public async Task<ActionResult<ChallengePageResponse<ChallengeResultEntry>>> Results(Guid challengeId,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        ValidatePagination(page, pageSize);
        return Ok(await _mediator.Send(new GetChallengeResultsQuery(User.GetRequiredUserId(), challengeId, page, pageSize)));
    }

    private static void ValidatePagination(int page, int pageSize)
    {
        if (page < 1 || pageSize is < 1 or > 100)
            throw new DomainException("Page must be at least one and page size must be between 1 and 100.");
    }
}
