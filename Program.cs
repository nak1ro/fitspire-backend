using backend.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using backend.Modules.Auth;
using backend.Modules.Shared;
using backend.Modules.User.Domain;
using backend.Modules.User.Mappings;
using backend.Modules.Workout;
using backend.Modules.Goal;
using backend.Modules.Social;
using backend.Modules.Goal.Data;
using backend.Modules.Shared.Middleware;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var dataProtectionKeysPath = Path.Combine(Path.GetTempPath(), "fitspire-data-protection-keys");
Directory.CreateDirectory(dataProtectionKeysPath);
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath));

// Modules
builder.Services.AddDataModule(builder.Configuration);
builder.Services.AddAuthModule(builder.Configuration);
builder.Services.AddSharedModule(builder.Configuration);
builder.Services.AddWorkoutModule();
builder.Services.AddGoalModule();
builder.Services.AddSocialModule();

// MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.AddEndpointsApiExplorer();


builder.Services.AddControllers();

var corsPolicy = "FrontendPolicy";
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: corsPolicy, policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); 
    });
});


var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseStaticFiles();

app.UseHttpsRedirection();
app.UseCors(corsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;

    var context = serviceProvider.GetRequiredService<FitspireDbContext>();
    context.Database.Migrate();

    await RoleSeeder.SeedAsync(serviceProvider);
    await backend.Modules.Workout.Data.Seeding.ExerciseSeeder.SeedAsync(serviceProvider);
    await new GoalTypeSeeder(context).SeedAsync();
}

app.Run();
