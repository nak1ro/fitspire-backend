using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace backend.Data;

public class FitspireDbContextFactory : IDesignTimeDbContextFactory<FitspireDbContext>
{
    public FitspireDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables()
            .Build();
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection is not configured.");

        var options = new DbContextOptionsBuilder<FitspireDbContext>()
            .UseNpgsql(connectionString)
            .Options;
        return new FitspireDbContext(options);
    }
}
