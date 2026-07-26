namespace backend.Infrastructure.Startup;

public sealed class StartupOptions
{
    public const string SectionName = "Startup";

    public bool ApplyMigrationsOnStartup { get; init; }
}
