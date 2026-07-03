using backend.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using backend.Modules.Auth;
using backend.Modules.Shared;
using backend.Modules.User.Domain;
using backend.Modules.User.Mappings;
using backend.Modules.User;
using backend.Modules.Workout;
using backend.Modules.Goal;
using backend.Modules.Social;
using backend.Modules.Goal.Data;
using backend.Modules.Shared.Middleware;
using backend.Modules.Notification;
using backend.Modules.Progress;
using backend.Modules.Progress.Data;
using backend.Modules.Challenge;
using backend.Modules.Badge.Data;
using backend.Modules.Badge;
using backend.Modules.Media;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var webRootPath = Path.Combine(builder.Environment.ContentRootPath, "wwwroot");
Directory.CreateDirectory(webRootPath);
builder.Environment.WebRootPath = webRootPath;
builder.Environment.WebRootFileProvider = new PhysicalFileProvider(webRootPath);

var dataProtectionKeysPath = Path.Combine(Path.GetTempPath(), "fitspire-data-protection-keys");
Directory.CreateDirectory(dataProtectionKeysPath);
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath));

// Modules
builder.Services.AddDataModule(builder.Configuration);
builder.Services.AddAuthModule(builder.Configuration);
builder.Services.AddSharedModule(builder.Configuration);
builder.Services.AddMediaModule(builder.Configuration);
builder.Services.AddUserModule();
builder.Services.AddWorkoutModule();
builder.Services.AddProgressModule();
builder.Services.AddGoalModule();
builder.Services.AddChallengeModule();
builder.Services.AddBadgeModule();
builder.Services.AddSocialModule();
builder.Services.AddNotificationModule();

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
    await MetricDefinitionSeeder.SeedAsync(context);
    await BadgeSeeder.SeedAsync(context);
    await new GoalTypeSeeder(context).SeedAsync();
}

app.Run();
