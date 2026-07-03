using backend.Modules.Media.Contracts;
using backend.Modules.Shared.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Modules.Media;

[ApiController]
[Route("api/media/uploads")]
[Authorize]
public class MediaController : ControllerBase
{
    private readonly IMediaUploadService _mediaUploadService;
    private readonly IValidator<InitiateMediaUploadRequest> _initiateValidator;

    public MediaController(
        IMediaUploadService mediaUploadService,
        IValidator<InitiateMediaUploadRequest> initiateValidator)
    {
        _mediaUploadService = mediaUploadService;
        _initiateValidator = initiateValidator;
    }

    [HttpPost]
    public async Task<ActionResult<MediaUploadSessionResponse>> Initiate(
        [FromBody] InitiateMediaUploadRequest request,
        CancellationToken cancellationToken)
    {
        await _initiateValidator.ValidateAndThrowAsync(request, cancellationToken);
        var response = await _mediaUploadService.InitiateAsync(request, User.GetRequiredUserId(), cancellationToken);
        return Ok(response);
    }

    [HttpPost("{mediaAssetId:guid}/complete")]
    public async Task<ActionResult<MediaAssetStatusResponse>> Complete(Guid mediaAssetId, CancellationToken cancellationToken)
    {
        var response = await _mediaUploadService.CompleteAsync(mediaAssetId, User.GetRequiredUserId(), cancellationToken);
        return Ok(response);
    }

    [HttpGet("{mediaAssetId:guid}")]
    public async Task<ActionResult<MediaAssetStatusResponse>> GetStatus(Guid mediaAssetId, CancellationToken cancellationToken)
    {
        var response = await _mediaUploadService.GetStatusAsync(mediaAssetId, User.GetRequiredUserId(), cancellationToken);
        return Ok(response);
    }

    [HttpDelete("{mediaAssetId:guid}")]
    public async Task<IActionResult> Abort(Guid mediaAssetId, CancellationToken cancellationToken)
    {
        await _mediaUploadService.AbortAsync(mediaAssetId, User.GetRequiredUserId(), cancellationToken);
        return NoContent();
    }
}
