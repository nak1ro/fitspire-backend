using backend.Data;
using backend.Modules.Challenge.Domain;
using backend.Modules.Challenge.Domain.Constants;
using backend.Modules.Shared.Domain;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.Challenge.Services;

public interface IChallengeAccessService
{
    Task EnsureCanViewAsync(UserChallenge challenge, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> CanJoinAsync(UserChallenge challenge, Guid userId, CancellationToken cancellationToken = default);
}

public class ChallengeAccessService : IChallengeAccessService
{
    private readonly FitspireDbContext _context;

    public ChallengeAccessService(FitspireDbContext context) => _context = context;

    public async Task EnsureCanViewAsync(UserChallenge challenge, Guid userId, CancellationToken cancellationToken = default)
    {
        if (challenge.Visibility == ChallengeVisibilities.Public || challenge.CreatedBy == userId) return;

        var wasParticipant = await _context.ChallengeParticipants.AnyAsync(item => item.ChallengeId == challenge.Id && item.UserId == userId, cancellationToken);
        if (wasParticipant) return;

        if (challenge.Visibility == ChallengeVisibilities.FollowersOnly &&
            await FollowsCreatorAsync(challenge.CreatedBy, userId, cancellationToken)) return;

        if (challenge.Visibility == ChallengeVisibilities.InviteOnly &&
            await _context.ChallengeInvitations.AnyAsync(item => item.ChallengeId == challenge.Id && item.InvitedUserId == userId &&
                (item.Status == ChallengeInvitationStatuses.Pending || item.Status == ChallengeInvitationStatuses.Accepted), cancellationToken)) return;

        throw new UnauthorizedAccessException("You do not have access to this challenge.");
    }

    public async Task<bool> CanJoinAsync(UserChallenge challenge, Guid userId, CancellationToken cancellationToken = default)
    {
        if (!challenge.IsJoinOpen(DateTime.UtcNow) || challenge.CreatedBy == userId) return false;
        if (await _context.ChallengeParticipants.AnyAsync(item => item.ChallengeId == challenge.Id && item.UserId == userId &&
                item.Status == ChallengeParticipantStatuses.Active, cancellationToken))
            return false;
        var activeCount = await _context.ChallengeParticipants.CountAsync(item => item.ChallengeId == challenge.Id && item.Status == ChallengeParticipantStatuses.Active, cancellationToken);
        if (activeCount >= challenge.ParticipantLimit) return false;

        return challenge.Visibility switch
        {
            ChallengeVisibilities.Public => true,
            ChallengeVisibilities.FollowersOnly => await FollowsCreatorAsync(challenge.CreatedBy, userId, cancellationToken),
            ChallengeVisibilities.InviteOnly => await _context.ChallengeInvitations.AnyAsync(item => item.ChallengeId == challenge.Id &&
                item.InvitedUserId == userId && item.Status == ChallengeInvitationStatuses.Accepted, cancellationToken),
            _ => false
        };
    }

    private Task<bool> FollowsCreatorAsync(Guid creatorId, Guid userId, CancellationToken cancellationToken) =>
        _context.Followers.AnyAsync(item => item.FollowerId == userId && item.FollowedId == creatorId, cancellationToken);
}
