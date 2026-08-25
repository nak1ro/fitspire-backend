namespace backend.Modules.DemoData.Services;

public enum DemoDataSeedState
{
    NotStarted,
    Running,
    Completed,
    Failed,
}

// In-memory, single-instance progress tracker for the one-off seed run. Whether the hero account
// exists is not a reliable "done" signal — it's created in the very first step, long before the
// pipeline actually finishes, so a crash partway through would otherwise look identical to success.
public interface IDemoDataSeedProgress
{
    DemoDataSeedState State { get; }
    string? ErrorMessage { get; }
    void MarkRunning();
    void MarkCompleted();
    void MarkFailed(string errorMessage);
}

public class DemoDataSeedProgress : IDemoDataSeedProgress
{
    private volatile DemoDataSeedState _state = DemoDataSeedState.NotStarted;
    private volatile string? _errorMessage;

    public DemoDataSeedState State => _state;
    public string? ErrorMessage => _errorMessage;

    public void MarkRunning()
    {
        _state = DemoDataSeedState.Running;
        _errorMessage = null;
    }

    public void MarkCompleted() => _state = DemoDataSeedState.Completed;

    public void MarkFailed(string errorMessage)
    {
        _state = DemoDataSeedState.Failed;
        _errorMessage = errorMessage;
    }
}
