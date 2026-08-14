namespace backend.Infrastructure.Startup;

public sealed class AdministrationOptions
{
    public const string SectionName = "Administration";

    public string[] InitialAdminEmails { get; init; } = [];
}
