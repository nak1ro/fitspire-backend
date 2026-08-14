using backend.Data;
using backend.Infrastructure.Hosting;
using backend.Infrastructure.Startup;
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
using backend.Modules.BodyTracking;
using backend.Modules.Media;
using backend.Modules.Moderation;
using backend.Modules.Nutrition;
using backend.Modules.AiCoaching;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var webRootPath = Path.Combine(builder.Environment.ContentRootPath, "wwwroot");
Directory.CreateDirectory(webRootPath);
builder.Environment.WebRootPath = webRootPath;
builder.Environment.WebRootFileProvider = new PhysicalFileProvider(webRootPath);

builder.Services.AddFitspireDataProtection(builder.Configuration, builder.Environment);
builder.Services.AddStartupInitialization(builder.Configuration);

// Modules
builder.Services.AddDataModule(builder.Configuration);
builder.Services.AddAuthModule(builder.Configuration);
builder.Services.AddSharedModule(builder.Configuration);
builder.Services.AddMediaModule(builder.Configuration);
builder.Services.AddModerationModule();
builder.Services.AddUserModule();
builder.Services.AddWorkoutModule();
builder.Services.AddProgressModule();
builder.Services.AddBodyTrackingModule();
builder.Services.AddNutritionModule();
builder.Services.AddAiCoachingModule(builder.Configuration);
builder.Services.AddGoalModule();
builder.Services.AddChallengeModule();
builder.Services.AddBadgeModule();
builder.Services.AddSocialModule();
builder.Services.AddNotificationModule();

// MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.AddControllers();
builder.Services.AddOperationalHealthChecks();

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

app.UseProductionForwardedHeaders(app.Environment);
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseStaticFiles();

app.UseHttpsRedirection();
app.UseCors(corsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.MapOperationalHealthChecks();
app.MapControllers();

await app.InitializeStartupAsync();

app.Run();
