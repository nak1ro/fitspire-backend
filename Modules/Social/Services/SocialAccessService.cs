using backend.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.Social.Services;

public class SocialAccessService : ISocialAccessService
{
    private readonly FitspireDbContext _context;

    public SocialAccessService(FitspireDbContext context)
    {
        _context = context;
    }

    public async Task<bool> CanViewProtectedContentAsync(
        Guid viewerUserId,
        Guid ownerUserId,
        CancellationToken cancellationToken = default)
    {
        if (viewerUserId == ownerUserId)
            return true;

        var isPrivate = await _context.Users
            .Where(user => user.Id == ownerUserId)
            .Select(user => (bool?)user.IsPrivate)
            .FirstOrDefaultAsync(cancellationToken);

        if (isPrivate is null)
            return false;
        if (!isPrivate.Value)
            return true;

        return await _context.Followers.AnyAsync(
            follow => follow.FollowerId == viewerUserId && follow.FollowedId == ownerUserId,
            cancellationToken);
    }
}
