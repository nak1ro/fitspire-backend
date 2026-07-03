using backend.Modules.Media.Infrastructure;

namespace backend.Modules.Media.Services;

public class MediaStorageInitializer : IHostedService
{
    private readonly AzureMediaObjectStorage _storage;

    public MediaStorageInitializer(AzureMediaObjectStorage storage)
    {
        _storage = storage;
    }

    public Task StartAsync(CancellationToken cancellationToken) => _storage.EnsureContainerAsync(cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
