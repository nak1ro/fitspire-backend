using backend.Data;
using backend.Modules.Shared.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.Badge;

public record BadgeResponse(Guid Id, string Code, string Name, string? Description, string Tier, bool IsEarned, DateTime? AwardedAt, int? FeaturedOrder);
public record SetFeaturedBadgesRequest(List<Guid> BadgeIds);

[ApiController]
[Route("api/badges")]
[Authorize]
public class BadgeController : ControllerBase
{
    private readonly FitspireDbContext _context; public BadgeController(FitspireDbContext context) => _context = context;
    [HttpGet]
    public async Task<ActionResult<List<BadgeResponse>>> Catalogue()
    {
        var userId = User.GetRequiredUserId();
        return Ok(await _context.Badges.Where(badge => badge.IsActive).OrderBy(badge => badge.DisplayOrder).Select(badge => new BadgeResponse(badge.Id, badge.Code, badge.Name, badge.Description, badge.Tier,
            badge.UserBadges.Any(award => award.UserId == userId), badge.UserBadges.Where(award => award.UserId == userId).Select(award => (DateTime?)award.AwardedAt).FirstOrDefault(), badge.UserBadges.Where(award => award.UserId == userId).Select(award => award.FeaturedOrder).FirstOrDefault())).ToListAsync());
    }
    [HttpGet("mine")] public async Task<ActionResult<List<BadgeResponse>>> Mine() { var userId = User.GetRequiredUserId(); return Ok(await _context.UserBadges.Where(award => award.UserId == userId).Include(award => award.AchievementBadge).OrderBy(award => award.AwardedAt).Select(award => new BadgeResponse(award.BadgeId, award.AchievementBadge.Code, award.AchievementBadge.Name, award.AchievementBadge.Description, award.AchievementBadge.Tier, true, award.AwardedAt, award.FeaturedOrder)).ToListAsync()); }
    [HttpPut("featured")]
    public async Task<IActionResult> SetFeatured(SetFeaturedBadgesRequest request)
    {
        var userId = User.GetRequiredUserId(); if (request.BadgeIds.Count > 5 || request.BadgeIds.Distinct().Count() != request.BadgeIds.Count) return BadRequest("Select up to five unique earned badges.");
        var awards = await _context.UserBadges.Where(award => award.UserId == userId).ToListAsync();
        if (request.BadgeIds.Any(id => awards.All(award => award.BadgeId != id))) return BadRequest("All featured badges must be earned.");
        foreach (var award in awards) award.FeaturedOrder = null;
        for (var index = 0; index < request.BadgeIds.Count; index++) awards.Single(award => award.BadgeId == request.BadgeIds[index]).FeaturedOrder = index + 1;
        await _context.SaveChangesAsync(); return NoContent();
    }
}
