using backend.Modules.Badge.Contracts;
using backend.Modules.Badge.Features;
using backend.Modules.Shared.Extensions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Modules.Badge;

[ApiController]
[Route("api/badges")]
[Authorize]
public class BadgeController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IValidator<BadgeCatalogueFilter> _catalogueValidator;
    private readonly IValidator<BadgeCollectionFilter> _collectionValidator;
    private readonly IValidator<SetFeaturedBadgesRequest> _featuredValidator;

    public BadgeController(IMediator mediator, IValidator<BadgeCatalogueFilter> catalogueValidator,
        IValidator<BadgeCollectionFilter> collectionValidator, IValidator<SetFeaturedBadgesRequest> featuredValidator)
    {
        _mediator = mediator;
        _catalogueValidator = catalogueValidator;
        _collectionValidator = collectionValidator;
        _featuredValidator = featuredValidator;
    }

    [HttpGet]
    public async Task<ActionResult<BadgePageResponse<BadgeCatalogueItemResponse>>> Catalogue([FromQuery] BadgeCatalogueFilter filter)
    {
        await _catalogueValidator.ValidateAndThrowAsync(filter);
        return Ok(await _mediator.Send(new GetBadgeCatalogueQuery(User.GetRequiredUserId(), filter)));
    }

    [HttpGet("mine")]
    public async Task<ActionResult<BadgePageResponse<EarnedBadgeResponse>>> Mine([FromQuery] BadgeCollectionFilter filter)
    {
        await _collectionValidator.ValidateAndThrowAsync(filter);
        return Ok(await _mediator.Send(new GetMyBadgesQuery(User.GetRequiredUserId(), filter)));
    }

    [HttpPut("featured")]
    public async Task<IActionResult> SetFeatured(SetFeaturedBadgesRequest request)
    {
        await _featuredValidator.ValidateAndThrowAsync(request);
        await _mediator.Send(new SetFeaturedBadgesCommand(User.GetRequiredUserId(), request.BadgeIds));
        return NoContent();
    }
}
