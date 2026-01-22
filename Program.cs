using backend.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using backend.Modules.Auth;
using backend.Modules.Shared;
using backend.Modules.User.Domain;
using backend.Modules.User.Mappings;
using backend.Modules.Workout;
using backend.Modules.Goal;

var builder = WebApplication.CreateBuilder(args);

// Modules
builder.Services.AddDataModule(builder.Configuration);
builder.Services.AddAuthModule(builder.Configuration);
builder.Services.AddSharedModule(builder.Configuration);
builder.Services.AddWorkoutModule();
builder.Services.AddGoalModule();

// MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.AddEndpointsApiExplorer();


builder.Services.AddControllers();

var corsPolicy = "FrontendPolicy";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: corsPolicy, policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:8081"  
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); 
    });
});


var app = builder.Build();

app.UseStaticFiles();

app.UseHttpsRedirection();
app.UseCors(corsPolicy);
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;

    var context = serviceProvider.GetRequiredService<FitspireDbContext>();
    context.Database.Migrate();

    await RoleSeeder.SeedAsync(serviceProvider);
    await backend.Modules.Workout.Data.Seeding.ExerciseSeeder.SeedAsync(serviceProvider);
}

app.Run();