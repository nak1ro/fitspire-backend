using System.Text.Json;
using backend.Data;
using backend.Modules.Media.Domain;
using backend.Modules.Moderation.Domain;
using backend.Modules.Shared.Domain;
using backend.Modules.Social.Services;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.Moderation.Services;

public interface IModerationTargetResolver
{
    Task<ModerationTarget> ResolveAsync(
        Guid reporterUserId,
        ModerationReportTargetType targetType,
        Guid targetId,
        CancellationToken cancellationToken = default);
}

public sealed record ModerationTarget(
    Guid SubjectUserId,
    ModerationReportTargetType TargetType,
    Guid TargetId,
    ModerationMediaContext? MediaContext,
    string SnapshotJson);

public sealed class ModerationTargetResolver : IModerationTargetResolver
{
    private const string SnapshotVersion = "v1";

    private readonly FitspireDbContext _context;
    private readonly ISocialAccessService _socialAccess;

    public ModerationTargetResolver(FitspireDbContext context, ISocialAccessService socialAccess)
    {
        _context = context;
        _socialAccess = socialAccess;
    }

    public Task<ModerationTarget> ResolveAsync(
        Guid reporterUserId,
        ModerationReportTargetType targetType,
        Guid targetId,
        CancellationToken cancellationToken = default) => targetType switch
    {
        ModerationReportTargetType.Profile => ResolveProfileAsync(reporterUserId, targetId, cancellationToken),
        ModerationReportTargetType.Post => ResolvePostAsync(reporterUserId, targetId, cancellationToken),
        ModerationReportTargetType.Comment => ResolveCommentAsync(reporterUserId, targetId, cancellationToken),
        ModerationReportTargetType.Media => ResolveMediaAsync(reporterUserId, targetId, cancellationToken),
        _ => throw new DomainException("Unsupported moderation report target.")
    };

    private async Task<ModerationTarget> ResolveProfileAsync(Guid reporterUserId, Guid targetId, CancellationToken cancellationToken)
    {
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(candidate => candidate.Id == targetId, cancellationToken)
            ?? throw new NotFoundException("Report target was not found.");
        RejectSelfReport(reporterUserId, user.Id);

        return new ModerationTarget(user.Id, ModerationReportTargetType.Profile, user.Id, null,
            Serialize(new
            {
                version = SnapshotVersion,
                targetType = ModerationReportTargetType.Profile,
                subject = UserSnapshot.From(user),
                user.Bio,
                profilePictureMediaId = user.ProfilePictureMediaId
            }));
    }

    private async Task<ModerationTarget> ResolvePostAsync(Guid reporterUserId, Guid targetId, CancellationToken cancellationToken)
    {
        var post = await _context.Posts.AsNoTracking()
            .Include(candidate => candidate.User)
            .Include(candidate => candidate.Media)
            .FirstOrDefaultAsync(candidate => candidate.Id == targetId && candidate.ModerationRemovedAtUtc == null, cancellationToken)
            ?? throw new NotFoundException("Report target was not found.");
        await EnsureVisiblePostAsync(reporterUserId, post.UserId, cancellationToken);

        return new ModerationTarget(post.UserId, ModerationReportTargetType.Post, post.Id, null,
            Serialize(new
            {
                version = SnapshotVersion,
                targetType = ModerationReportTargetType.Post,
                subject = UserSnapshot.From(post.User),
                post = new
                {
                    post.Id,
                    type = post.Type.ToString(),
                    post.Content,
                    post.CreatedAt,
                    mediaAssetIds = post.Media.Select(media => media.MediaAssetId).ToArray()
                }
            }));
    }

    private async Task<ModerationTarget> ResolveCommentAsync(Guid reporterUserId, Guid targetId, CancellationToken cancellationToken)
    {
        var comment = await _context.Comments.AsNoTracking()
            .Include(candidate => candidate.User)
            .Include(candidate => candidate.Post)
            .FirstOrDefaultAsync(candidate => candidate.Id == targetId && candidate.ModerationRemovedAtUtc == null && candidate.Post.ModerationRemovedAtUtc == null,
                cancellationToken)
            ?? throw new NotFoundException("Report target was not found.");
        await EnsureVisiblePostAsync(reporterUserId, comment.Post.UserId, cancellationToken);

        return new ModerationTarget(comment.UserId, ModerationReportTargetType.Comment, comment.Id, null,
            Serialize(new
            {
                version = SnapshotVersion,
                targetType = ModerationReportTargetType.Comment,
                subject = UserSnapshot.From(comment.User),
                comment = new
                {
                    comment.Id,
                    comment.PostId,
                    comment.Content,
                    comment.RootCommentId,
                    comment.ReplyToCommentId,
                    comment.CreatedAt
                }
            }));
    }

    private async Task<ModerationTarget> ResolveMediaAsync(Guid reporterUserId, Guid targetId, CancellationToken cancellationToken)
    {
        var profileTarget = await ResolveProfilePictureAsync(reporterUserId, targetId, cancellationToken);
        return profileTarget ?? await ResolvePostImageAsync(reporterUserId, targetId, cancellationToken)
            ?? throw new NotFoundException("Report target was not found.");
    }

    private async Task<ModerationTarget?> ResolveProfilePictureAsync(Guid reporterUserId, Guid targetId, CancellationToken cancellationToken)
    {
        var user = await _context.Users.AsNoTracking()
            .Include(candidate => candidate.ProfilePictureMedia)
            .FirstOrDefaultAsync(candidate => candidate.ProfilePictureMediaId == targetId, cancellationToken);
        var asset = user?.ProfilePictureMedia;
        if (user is null || asset is null || asset.Purpose != MediaPurpose.ProfilePicture || asset.IsModerationRemoved)
            return null;

        RejectSelfReport(reporterUserId, user.Id);
        return new ModerationTarget(user.Id, ModerationReportTargetType.Media, asset.Id, ModerationMediaContext.ProfilePicture,
            Serialize(new
            {
                version = SnapshotVersion,
                targetType = ModerationReportTargetType.Media,
                mediaContext = ModerationMediaContext.ProfilePicture,
                subject = UserSnapshot.From(user),
                media = MediaSnapshot.From(asset)
            }));
    }

    private async Task<ModerationTarget?> ResolvePostImageAsync(Guid reporterUserId, Guid targetId, CancellationToken cancellationToken)
    {
        var postMedia = await _context.PostMedia.AsNoTracking()
            .Include(candidate => candidate.MediaAsset)
            .Include(candidate => candidate.Post)
                .ThenInclude(post => post.User)
            .FirstOrDefaultAsync(candidate => candidate.MediaAssetId == targetId && candidate.MediaAsset.ModerationRemovedAtUtc == null &&
                                              candidate.Post.ModerationRemovedAtUtc == null, cancellationToken);
        if (postMedia is null || postMedia.MediaAsset.Purpose != MediaPurpose.PostImage)
            return null;

        await EnsureVisiblePostAsync(reporterUserId, postMedia.Post.UserId, cancellationToken);
        return new ModerationTarget(postMedia.Post.UserId, ModerationReportTargetType.Media, postMedia.MediaAssetId,
            ModerationMediaContext.PostImage,
            Serialize(new
            {
                version = SnapshotVersion,
                targetType = ModerationReportTargetType.Media,
                mediaContext = ModerationMediaContext.PostImage,
                subject = UserSnapshot.From(postMedia.Post.User),
                postId = postMedia.PostId,
                media = MediaSnapshot.From(postMedia.MediaAsset)
            }));
    }

    private async Task EnsureVisiblePostAsync(Guid reporterUserId, Guid ownerUserId, CancellationToken cancellationToken)
    {
        RejectSelfReport(reporterUserId, ownerUserId);
        if (!await _socialAccess.CanViewProtectedContentAsync(reporterUserId, ownerUserId, cancellationToken))
            throw new NotFoundException("Report target was not found.");
    }

    private static void RejectSelfReport(Guid reporterUserId, Guid subjectUserId)
    {
        if (reporterUserId == subjectUserId)
            throw new DomainException("Users cannot report themselves or their own content.");
    }

    private static string Serialize<T>(T value) => JsonSerializer.Serialize(value);

    private sealed record UserSnapshot(Guid Id, string UserName, string DisplayName)
    {
        public static UserSnapshot From(Modules.User.Domain.AppUser user) => new(user.Id, user.UserName ?? string.Empty, user.DisplayName);
    }

    private sealed record MediaSnapshot(Guid Id, string Purpose, string ContentType, long DeclaredSizeBytes)
    {
        public static MediaSnapshot From(MediaAsset asset) => new(asset.Id, asset.Purpose.ToString(), asset.DeclaredContentType,
            asset.DeclaredSizeBytes);
    }
}
