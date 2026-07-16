using backend.Modules.BodyTracking.Contracts;
using backend.Modules.BodyTracking.Features;
using backend.Modules.Shared.Extensions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Modules.BodyTracking;

[ApiController]
[Route("api/body-check-ins")]
[Authorize]
public class BodyCheckInsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IValidator<BodyCheckInHistoryFilter> _historyValidator;
    private readonly IValidator<BodyCheckInSummaryFilter> _summaryValidator;

    public BodyCheckInsController(IMediator mediator, IValidator<BodyCheckInHistoryFilter> historyValidator,
        IValidator<BodyCheckInSummaryFilter> summaryValidator)
    {
        _mediator = mediator;
        _historyValidator = historyValidator;
        _summaryValidator = summaryValidator;
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateBodyCheckInRequest request, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(new CreateBodyCheckInCommand(User.GetRequiredUserId(), request), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpGet("latest")]
    public async Task<ActionResult<BodyCheckInResponse>> GetLatest(CancellationToken cancellationToken)
    {
        var checkIn = await _mediator.Send(new GetLatestBodyCheckInQuery(User.GetRequiredUserId()), cancellationToken);
        return checkIn is null ? NoContent() : Ok(checkIn);
    }

    [HttpGet("summary")]
    public async Task<ActionResult<BodyCheckInSummaryResponse>> GetSummary([FromQuery] BodyCheckInSummaryFilter filter,
        CancellationToken cancellationToken)
    {
        await _summaryValidator.ValidateAndThrowAsync(filter, cancellationToken);
        return Ok(await _mediator.Send(new GetBodyCheckInSummaryQuery(User.GetRequiredUserId(), filter), cancellationToken));
    }

    [HttpGet]
    public async Task<ActionResult<BodyCheckInPageResponse>> GetHistory([FromQuery] BodyCheckInHistoryFilter filter,
        CancellationToken cancellationToken)
    {
        await _historyValidator.ValidateAndThrowAsync(filter, cancellationToken);
        return Ok(await _mediator.Send(new GetBodyCheckInHistoryQuery(User.GetRequiredUserId(), filter), cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BodyCheckInResponse>> GetById(Guid id, CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new GetBodyCheckInQuery(User.GetRequiredUserId(), id), cancellationToken));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBodyCheckInRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new UpdateBodyCheckInCommand(User.GetRequiredUserId(), id, request), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteBodyCheckInCommand(User.GetRequiredUserId(), id), cancellationToken);
        return NoContent();
    }
}
