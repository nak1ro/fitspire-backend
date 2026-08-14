using backend.Modules.Moderation.Contracts;
using backend.Modules.Shared.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Modules.Moderation;

[ApiController]
[Authorize]
[Route("api/moderation/reports")]
public sealed class ModerationController : ControllerBase
{
    private readonly IModerationReportService _service;
    private readonly IValidator<CreateModerationReportRequest> _validator;

    public ModerationController(IModerationReportService service, IValidator<CreateModerationReportRequest> validator)
    {
        _service = service;
        _validator = validator;
    }

    [HttpPost]
    public async Task<ActionResult<ModerationReportSubmissionResponse>> Create(
        [FromBody] CreateModerationReportRequest request,
        CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);
        var response = await _service.CreateAsync(User.GetRequiredUserId(), request, cancellationToken);
        return response.AlreadyReported ? Ok(response) : StatusCode(StatusCodes.Status201Created, response);
    }
}
