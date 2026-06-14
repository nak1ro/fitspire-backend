namespace backend.Modules.Social.Services;

public interface ISocialAccessService
{
    Task<bool> CanViewProtectedContentAsync(
        Guid viewerUserId,
        Guid ownerUserId,
        CancellationToken cancellationToken = default);
}
