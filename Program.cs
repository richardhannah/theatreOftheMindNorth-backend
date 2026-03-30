using Microsoft.EntityFrameworkCore;
using TheatreOfTheMind.Data;
using TheatreOfTheMind.Hubs;
using TheatreOfTheMind.Repositories;
using TheatreOfTheMind.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .SetIsOriginAllowed(_ => true);
    });
});

builder.Services.AddControllers();
builder.Services.AddSignalR()
    .AddJsonProtocol(options =>
    {
        options.PayloadSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

var dbHost = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
var dbPort = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";
var dbName = Environment.GetEnvironmentVariable("DB_DATABASE") ?? "theatreofthemind";
var dbUser = Environment.GetEnvironmentVariable("DB_USERNAME") ?? "totm";
var dbPass = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "totm_dev";
var connectionString = $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPass}";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<ILoginRepository, LoginRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICharacterRepository, CharacterRepository>();
builder.Services.AddScoped<IStashRepository, StashRepository>();
builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddHostedService<SceneBackupService>();

var app = builder.Build();

try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Failed to apply database migrations. The app will start but database features may not work.");
}

app.UseCors();
app.MapControllers();
app.MapHub<ChatHub>("/chatHub");
app.MapHub<VttHub>("/vttHub");

app.MapGet("/healthz", () => Results.Ok());

app.Run();
