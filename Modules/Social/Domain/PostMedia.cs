using backend.Modules.Media.Domain;
using backend.Modules.Shared.Domain;

namespace backend.Modules.Social.Domain;

public class PostMedia : Entity<Guid>
{
    public Guid PostId { get; private set; }
    public Guid MediaAssetId { get; private set; }
    public int Order { get; private set; }

    public Post Post { get; private set; } = null!;
    public MediaAsset MediaAsset { get; private set; } = null!;

    private PostMedia()
    {
    }

    internal static PostMedia Create(Guid postId, Guid mediaAssetId, int order)
    {
        if (postId == Guid.Empty || mediaAssetId == Guid.Empty)
            throw new DomainException("Post media requires a post and media asset.");

        if (order is < 0 or >= MediaPolicies.MaximumPostImages)
            throw new DomainException("Post media order is outside the supported range.");

        return new PostMedia
        {
            Id = Guid.NewGuid(),
            PostId = postId,
            MediaAssetId = mediaAssetId,
            Order = order,
            CreatedAt = DateTime.UtcNow
        };
    }

    internal void MoveTo(int order)
    {
        if (order < 0)
            throw new DomainException("Post media order cannot be negative.");

        Order = order;
        UpdatedAt = DateTime.UtcNow;
    }
}
