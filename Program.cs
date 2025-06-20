using System.Text;
using backend.Data;
using backend.Modules.Auth.Services;
using backend.Modules.Shared.Service;
using backend.Modules.User.Domain;
using backend.Modules.User.Mappings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Resend;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<FitspireDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<AppUser, IdentityRole<Guid>>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 6;
    })
    .AddEntityFrameworkStores<FitspireDbContext>()
    .AddDefaultTokenProviders()
    .AddRoles<IdentityRole<Guid>>();

var jwtSection = builder.Configuration.GetSection("JWT");
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSection["Issuer"],

            ValidateAudience = true,
            ValidAudience = jwtSection["Audience"],

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSection["SigningKey"])),

            ValidateLifetime = true
        };
    });

builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = "<your-google-client-id>";
        options.ClientSecret = "<your-google-client-secret>";
    });

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IEmailService, ResendEmailService>();
builder.Services.AddHttpClient();
builder.Services.Configure<ResendClientOptions>(builder.Configuration.GetSection("Resend"));
builder.Services.AddScoped<ResendClient>(); // 
builder.Services.AddScoped<IEmailService, ResendEmailService>();
builder.Services.AddScoped<IBlobService, BlobService>(); // 
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddEndpointsApiExplorer();


builder.Services.AddControllers();

var app = builder.Build();

app.UseStaticFiles();

app.UseHttpsRedirection();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;

    var context = serviceProvider.GetRequiredService<FitspireDbContext>();
    context.Database.Migrate();

    await RoleSeeder.SeedAsync(serviceProvider);
}

app.Run();