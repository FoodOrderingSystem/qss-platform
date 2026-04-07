using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QSS.API.Hubs;
using QSS.Domain.Entities;
using QSS.Infrastructure.Data;
using QSS.Infrastructure.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Ensure the persistent data directory exists before the database is accessed
try
{
    Directory.CreateDirectory("/app/data");
}
catch (Exception ex)
{
    Console.Error.WriteLine($"[Startup] Warning: Could not create /app/data: {ex.Message}. Database writes may fail if the volume is not mounted.");
}

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=/app/data/qss.db"));

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "QSS-Platform-Super-Secret-Key-2024!";
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
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "QSS",
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "QSS",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };

    // SignalR JWT support
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) &&
                (path.StartsWithSegments("/hubs/chat") || path.StartsWithSegments("/hubs/notifications")))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// SignalR
builder.Services.AddSignalR();

// Application Services
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<QrCodeService>();

// CORS for the Web frontend
// In production, set Cors__AllowedOrigins__0=https://<web-service>.up.railway.app via Railway env vars
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:5001", "https://localhost:5001" };
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebFrontend", policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "QSS Platform API",
        Version = "v1",
        Description = "Quality Management System for Dental Practices"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Enter JWT token",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        await DbSeeder.SeedAsync(db, userManager, roleManager);
        logger.LogInformation("Database seeding completed successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Database seeding failed. The application will start with the existing database state.");
    }
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "QSS API v1");
    c.RoutePrefix = "swagger";
});

app.UseStaticFiles();
app.UseCors("AllowWebFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");
app.MapHub<NotificationHub>("/hubs/notifications");

// Health check endpoint for Railway — verifies DB is reachable.
// Always returns HTTP 200 so Railway does not mark the container as failed
// when the database is temporarily unavailable (e.g. first boot migration in progress).
app.MapGet("/health", async (ApplicationDbContext db) =>
{
    string dbStatus;
    string? dbError = null;
    try
    {
        await db.Database.CanConnectAsync();
        dbStatus = "connected";
    }
    catch (Exception ex)
    {
        dbStatus = "unreachable";
        dbError = ex.Message;
    }
    return Results.Ok(new
    {
        status = dbStatus == "connected" ? "healthy" : "degraded",
        database = dbStatus,
        error = dbError
    });
});

// Redirect root to swagger
app.MapGet("/", () => Results.Redirect("/swagger"));

// Use Railway's dynamic PORT env var if present, otherwise default to 8080
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Run($"http://+:{port}");
