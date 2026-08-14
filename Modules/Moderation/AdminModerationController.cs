using backend.Modules.Auth.Authorization;
using backend.Modules.Moderation.Contracts;
using backend.Modules.Shared.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Modules.Moderation;

[ApiController]
[Authorize(Policy = AppPolicies.AdminOnly)]
[Route("api/admin")]
public sealed class AdminModerationController : ControllerBase
{
    private readonly IAdminModerationService _service;
    private readonly IValidator<AdminModerationReportFilter> _filterValidator;
    private readonly IValidator<ResolveModerationReportRequest> _resolveValidator;

    public AdminModerationController(IAdminModerationService service,
        IValidator<AdminModerationReportFilter> filterValidator,
        IValidator<ResolveModerationReportRequest> resolveValidator)
    {
        _service = service;
        _filterValidator = filterValidator;
        _resolveValidator = resolveValidator;
    }

    [HttpGet("access")]
    public IActionResult GetAccess() => Ok(new { IsAdmin = true });

    [HttpGet("moderation/summary")]
    public async Task<ActionResult<AdminModerationQueueSummaryResponse>> GetSummary(CancellationToken cancellationToken) =>
        Ok(await _service.GetSummaryAsync(cancellationToken));

    [HttpGet("moderation/reports")]
    public async Task<ActionResult<AdminModerationReportPageResponse>> GetReports([FromQuery] AdminModerationReportFilter filter,
        CancellationToken cancellationToken)
    {
        await _filterValidator.ValidateAndThrowAsync(filter, cancellationToken);
        return Ok(await _service.GetReportsAsync(filter, cancellationToken));
    }

    [HttpGet("moderation/reports/{reportId:guid}")]
    public async Task<ActionResult<AdminModerationReportDetailResponse>> GetReport(Guid reportId, CancellationToken cancellationToken) =>
        Ok(await _service.GetReportAsync(reportId, cancellationToken));

    [HttpPost("moderation/reports/{reportId:guid}/resolve")]
    public async Task<ActionResult<AdminModerationReportDetailResponse>> Resolve(Guid reportId,
        [FromBody] ResolveModerationReportRequest request, CancellationToken cancellationToken)
    {
        await _resolveValidator.ValidateAndThrowAsync(request, cancellationToken);
        return Ok(await _service.ResolveAsync(User.GetRequiredUserId(), reportId, request, cancellationToken));
    }

    [HttpPost("moderation/reports/{reportId:guid}/restore-target")]
    public async Task<ActionResult<AdminModerationReportDetailResponse>> RestoreTarget(Guid reportId, CancellationToken cancellationToken) =>
        Ok(await _service.RestoreTargetAsync(User.GetRequiredUserId(), reportId, cancellationToken));

    [HttpPost("moderation/reports/{reportId:guid}/unsuspend-user")]
    public async Task<ActionResult<AdminModerationReportDetailResponse>> UnsuspendUser(Guid reportId, CancellationToken cancellationToken) =>
        Ok(await _service.UnsuspendUserAsync(User.GetRequiredUserId(), reportId, cancellationToken));
}
