using backend.Data;
using backend.Modules.Badge.Contracts;
using backend.Modules.Badge.Services;
using backend.Modules.Shared;
using backend.Modules.Shared.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.Badge.Features;

public record SetFeaturedBadgesCommand(Guid UserId, IReadOnlyList<Guid> BadgeIds) : IRequest;

public class SetFeaturedBadgesHandler : IRequestHandler<SetFeaturedBadgesCommand>
{
    private readonly FitspireDbContext _context;
    private readonly IBadgeTransactionService _transactions;
    private readonly IBadgeUserLockService _locks;
    private readonly IUnitOfWork _unitOfWork;

    public SetFeaturedBadgesHandler(FitspireDbContext context, IBadgeTransactionService transactions,
        IBadgeUserLockService locks, IUnitOfWork unitOfWork)
    {
        _context = context;
        _transactions = transactions;
        _locks = locks;
        _unitOfWork = unitOfWork;
    }

    public Task Handle(SetFeaturedBadgesCommand request, CancellationToken cancellationToken) =>
        _transactions.ExecuteAsync(token => ReplaceAsync(request, token), cancellationToken);

    private async Task ReplaceAsync(SetFeaturedBadgesCommand request, CancellationToken cancellationToken)
    {
        EnsureValidRequest(request.BadgeIds);
        await _locks.AcquireAsync(request.UserId, cancellationToken);
        var awards = await _context.UserBadges.Where(award => award.UserId == request.UserId).ToListAsync(cancellationToken);
        var byBadgeId = awards.ToDictionary(award => award.BadgeId);
        if (request.BadgeIds.Any(badgeId => !byBadgeId.ContainsKey(badgeId)))
            throw new DomainException("All featured badges must be earned.");

        foreach (var award in awards)
            award.ClearFeaturedOrder();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        for (var index = 0; index < request.BadgeIds.Count; index++)
            byBadgeId[request.BadgeIds[index]].SetFeaturedOrder(index + 1);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static void EnsureValidRequest(IReadOnlyList<Guid> badgeIds)
    {
        if (badgeIds.Count > 5 || badgeIds.Distinct().Count() != badgeIds.Count || badgeIds.Any(id => id == Guid.Empty))
            throw new DomainException("Select up to five unique earned badges.");
    }
}
