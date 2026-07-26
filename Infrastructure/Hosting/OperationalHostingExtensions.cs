using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace backend.Infrastructure.Hosting;

public static class OperationalHostingExtensions
{
    public static IServiceCollection AddOperationalHealthChecks(this IServiceCollection services) =>
        services.AddHealthChecks()
            .AddCheck<DatabaseReadinessHealthCheck>("postgresql", tags: ["ready"], timeout: TimeSpan.FromSeconds(5))
            .Services;

    public static IApplicationBuilder UseProductionForwardedHeaders(this IApplicationBuilder app, IHostEnvironment environment)
    {
        if (!environment.IsProduction())
            return app;

        var options = new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
            ForwardLimit = 1
        };
        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();
        return app.UseForwardedHeaders(options);
    }

    public static IEndpointRouteBuilder MapOperationalHealthChecks(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false
        }).AllowAnonymous();

        endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains("ready")
        }).AllowAnonymous();

        return endpoints;
    }
}
