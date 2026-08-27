using backend.Modules.Shared.Domain;
using backend.Modules.Shared.Extensions;
using backend.Modules.Social.Contracts.Profiles;
using backend.Modules.Social.Features.Profiles;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Modules.Social.Controllers;

[ApiController]
[Authorize]
[Route("api/social/users")]
public class SocialProfilesController : ControllerBase
{
    private readonly IMediator _mediator;

    public SocialProfilesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("search")]
    public async Task<ActionResult<List<SocialUserSummaryResponse>>> Search(
        [FromQuery] string query,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        ValidatePagination(page, pageSize);
        var users = await _mediator.Send(new SearchSocialUsersQuery(query, page, pageSize));
        return Ok(users);
    }

    [HttpGet("discover")]
    public async Task<ActionResult<List<DiscoverableSocialUserResponse>>> Discover(
        [FromQuery] string? query,
        [FromQuery] int limit = 5)
    {
        var users = await _mediator.Send(new DiscoverSocialUsersQuery(User.GetRequiredUserId(), query, limit));
        return Ok(users);
    }

    [HttpGet("{userId:guid}")]
    public async Task<ActionResult<SocialProfileResponse>> GetProfile(Guid userId)
    {
        var profile = await _mediator.Send(new GetSocialProfileQuery(User.GetRequiredUserId(), userId));
        return Ok(profile);
    }

    [HttpGet("{userId:guid}/followers")]
    public async Task<ActionResult<List<SocialUserSummaryResponse>>> GetFollowers(
        Guid userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        ValidatePagination(page, pageSize);
        var users = await _mediator.Send(new GetSocialConnectionsQuery(User.GetRequiredUserId(), userId, true, page, pageSize));
        return Ok(users);
    }

    [HttpGet("{userId:guid}/following")]
    public async Task<ActionResult<List<SocialUserSummaryResponse>>> GetFollowing(
        Guid userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        ValidatePagination(page, pageSize);
        var users = await _mediator.Send(new GetSocialConnectionsQuery(User.GetRequiredUserId(), userId, false, page, pageSize));
        return Ok(users);
    }

    private static void ValidatePagination(int page, int pageSize)
    {
        if (page < 1 || pageSize is < 1 or > 100)
            throw new DomainException("Page must be at least one and page size must be between 1 and 100.");
    }
}
