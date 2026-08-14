using backend.Modules.AiCoaching.Contracts;
using backend.Modules.AiCoaching.Domain;
using backend.Modules.AiCoaching.Services;
using backend.Modules.Shared.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Modules.AiCoaching;

[ApiController]
[Route("api/ai-coach")]
[Authorize]
public sealed class CoachInteractionsController : ControllerBase
{
    private readonly ICoachInteractionService _service;
    private readonly IValidator<CreateCoachThreadRequest> _createThreadValidator;
    private readonly IValidator<UpdateCoachThreadRequest> _updateThreadValidator;
    private readonly IValidator<CoachThreadHistoryFilter> _threadHistoryValidator;
    private readonly IValidator<SendCoachMessageRequest> _messageValidator;
    private readonly IValidator<CoachMessageHistoryFilter> _messageHistoryValidator;

    public CoachInteractionsController(ICoachInteractionService service, IValidator<CreateCoachThreadRequest> createThreadValidator,
        IValidator<UpdateCoachThreadRequest> updateThreadValidator, IValidator<CoachThreadHistoryFilter> threadHistoryValidator,
        IValidator<SendCoachMessageRequest> messageValidator, IValidator<CoachMessageHistoryFilter> messageHistoryValidator)
    {
        _service = service;
        _createThreadValidator = createThreadValidator;
        _updateThreadValidator = updateThreadValidator;
        _threadHistoryValidator = threadHistoryValidator;
        _messageValidator = messageValidator;
        _messageHistoryValidator = messageHistoryValidator;
    }

    [HttpPost("threads")]
    public async Task<ActionResult<CoachThreadResponse>> CreateThread([FromBody] CreateCoachThreadRequest request, CancellationToken cancellationToken)
    {
        await _createThreadValidator.ValidateAndThrowAsync(request, cancellationToken);
        var thread = await _service.CreateThreadAsync(User.GetRequiredUserId(), request, cancellationToken);
        return CreatedAtAction(nameof(GetThread), new { threadId = thread.Id }, thread);
    }

    [HttpGet("threads")]
    public async Task<ActionResult<CoachThreadPageResponse>> GetThreads([FromQuery] CoachThreadHistoryFilter filter, CancellationToken cancellationToken)
    {
        await _threadHistoryValidator.ValidateAndThrowAsync(filter, cancellationToken);
        return Ok(await _service.GetThreadsAsync(User.GetRequiredUserId(), filter, cancellationToken));
    }

    [HttpGet("threads/{threadId:guid}")]
    public async Task<ActionResult<CoachThreadResponse>> GetThread(Guid threadId, CancellationToken cancellationToken) =>
        Ok(await _service.GetThreadAsync(User.GetRequiredUserId(), threadId, cancellationToken));

    [HttpPatch("threads/{threadId:guid}")]
    public async Task<ActionResult<CoachThreadResponse>> RenameThread(Guid threadId, [FromBody] UpdateCoachThreadRequest request,
        CancellationToken cancellationToken)
    {
        await _updateThreadValidator.ValidateAndThrowAsync(request, cancellationToken);
        return Ok(await _service.RenameThreadAsync(User.GetRequiredUserId(), threadId, request, cancellationToken));
    }

    [HttpDelete("threads/{threadId:guid}")]
    public async Task<IActionResult> DeleteThread(Guid threadId, CancellationToken cancellationToken)
    {
        await _service.DeleteThreadAsync(User.GetRequiredUserId(), threadId, cancellationToken);
        return NoContent();
    }

    [HttpPost("threads/{threadId:guid}/messages")]
    public async Task<ActionResult<CoachQueuedExchangeResponse>> SendMessage(Guid threadId, [FromBody] SendCoachMessageRequest request,
        CancellationToken cancellationToken)
    {
        await _messageValidator.ValidateAndThrowAsync(request, cancellationToken);
        var exchange = await _service.SendMessageAsync(User.GetRequiredUserId(), threadId, request, cancellationToken);
        return exchange.Accepted ? Accepted(exchange) : Ok(exchange);
    }

    [HttpGet("threads/{threadId:guid}/messages")]
    public async Task<ActionResult<CoachMessageHistoryResponse>> GetMessages(Guid threadId, [FromQuery] CoachMessageHistoryFilter filter,
        CancellationToken cancellationToken)
    {
        await _messageHistoryValidator.ValidateAndThrowAsync(filter, cancellationToken);
        return Ok(await _service.GetMessagesAsync(User.GetRequiredUserId(), threadId, filter, cancellationToken));
    }

    [HttpGet("threads/{threadId:guid}/messages/{messageId:guid}")]
    public async Task<ActionResult<CoachMessageResponse>> GetMessage(Guid threadId, Guid messageId, CancellationToken cancellationToken) =>
        Ok(await _service.GetMessageAsync(User.GetRequiredUserId(), threadId, messageId, cancellationToken));

    [HttpPost("threads/{threadId:guid}/messages/{messageId:guid}/retry")]
    public async Task<ActionResult<CoachMessageResponse>> RetryMessage(Guid threadId, Guid messageId, CancellationToken cancellationToken) =>
        Accepted(await _service.RetryMessageAsync(User.GetRequiredUserId(), threadId, messageId, cancellationToken));

    [HttpPost("daily-briefings/today")]
    public async Task<ActionResult<DailyCoachBriefingResponse>> QueueDailyBriefing(CancellationToken cancellationToken)
    {
        var briefing = await _service.QueueDailyBriefingAsync(User.GetRequiredUserId(), cancellationToken);
        return briefing.Status is CoachGenerationStatus.Pending or CoachGenerationStatus.Processing ? Accepted(briefing) : Ok(briefing);
    }

    [HttpGet("daily-briefings/today")]
    public async Task<ActionResult<DailyCoachBriefingResponse>> GetTodayDailyBriefing(CancellationToken cancellationToken) =>
        Ok(await _service.GetTodayDailyBriefingAsync(User.GetRequiredUserId(), cancellationToken));

    [HttpGet("daily-briefings/{briefingId:guid}")]
    public async Task<ActionResult<DailyCoachBriefingResponse>> GetDailyBriefing(Guid briefingId, CancellationToken cancellationToken) =>
        Ok(await _service.GetDailyBriefingAsync(User.GetRequiredUserId(), briefingId, cancellationToken));

    [HttpPost("daily-briefings/{briefingId:guid}/retry")]
    public async Task<ActionResult<DailyCoachBriefingResponse>> RetryDailyBriefing(Guid briefingId, CancellationToken cancellationToken) =>
        Accepted(await _service.RetryDailyBriefingAsync(User.GetRequiredUserId(), briefingId, cancellationToken));
}
