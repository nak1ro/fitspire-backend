using backend.Modules.AiCoaching.Contracts;
using backend.Modules.AiCoaching.Services;
using backend.Modules.Shared.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Modules.AiCoaching;

[ApiController]
[Route("api/ai-coach/weekly-reports")]
[Authorize]
public sealed class AiCoachingController : ControllerBase
{
    private readonly IWeeklyCoachReportService _service;
    private readonly IValidator<GenerateWeeklyCoachReportRequest> _generationValidator;
    private readonly IValidator<WeeklyCoachReportHistoryFilter> _historyValidator;

    public AiCoachingController(IWeeklyCoachReportService service,
        IValidator<GenerateWeeklyCoachReportRequest> generationValidator,
        IValidator<WeeklyCoachReportHistoryFilter> historyValidator)
    {
        _service = service;
        _generationValidator = generationValidator;
        _historyValidator = historyValidator;
    }

    [HttpPost]
    public async Task<ActionResult<WeeklyCoachReportResponse>> Generate([FromBody] GenerateWeeklyCoachReportRequest request,
        CancellationToken cancellationToken)
    {
        await _generationValidator.ValidateAndThrowAsync(request, cancellationToken);
        var result = await _service.RequestGenerationAsync(User.GetRequiredUserId(), request, cancellationToken);
        return result.Accepted
            ? AcceptedAtAction(nameof(GetById), new { id = result.Report.Id }, result.Report)
            : Ok(result.Report);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<WeeklyCoachReportResponse>> GetById(Guid id, CancellationToken cancellationToken) =>
        Ok(await _service.GetAsync(User.GetRequiredUserId(), id, cancellationToken));

    [HttpGet]
    public async Task<ActionResult<WeeklyCoachReportPageResponse>> GetHistory([FromQuery] WeeklyCoachReportHistoryFilter filter,
        CancellationToken cancellationToken)
    {
        await _historyValidator.ValidateAndThrowAsync(filter, cancellationToken);
        return Ok(await _service.GetHistoryAsync(User.GetRequiredUserId(), filter, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _service.DeleteAsync(User.GetRequiredUserId(), id, cancellationToken);
        return NoContent();
    }
}
