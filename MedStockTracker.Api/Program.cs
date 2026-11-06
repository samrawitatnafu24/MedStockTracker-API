using System;
using System.Text;
using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using FluentValidation;
using MediatR;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Threading.RateLimiting;
using MedStockTracker.Application.Interfaces;
using MedStockTracker.Application.Services;
using MedStockTracker.Application.Behaviors;
using MedStockTracker.Infrastructure.Persistence.Data;
using MedStockTracker.Infrastructure.Persistence.Repositories;
using MedStockTracker.Infrastructure.Authentication;
using MedStockTracker.Infrastructure.Identity;
using MedStockTracker.Api.Middlewares;
using MedStockTracker.Api.Authorization;

var builder = WebApplication.CreateBuilder(args);

// =========================================================================
// 1. SERVICES CONFIGURATION (DEPENDENCY INJECTION)
// =========================================================================

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options
        .UseNpgsql(builder.Configuration.GetConnectionString("MedStockDb"))
        .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning))
        .LogTo(Console.WriteLine, LogLevel.Information)
        .EnableSensitiveDataLogging()
);

builder.Services.AddScoped<DbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(MedStockTracker.Application.Medicines.Commands.CreateMedicineCommand).Assembly);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

var targetApplicationAssembly = typeof(MedStockTracker.Application.Medicines.Commands.CreateMedicineCommand).Assembly;
builder.Services.AddValidatorsFromAssembly(targetApplicationAssembly);

builder.Services.AddScoped<IMedicineRepository, MedicineRepository>();
builder.Services.AddScoped<IMedicineService, MedicineService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

// 1. Initialize the Core Identity system configurations safely
builder.Services.AddIdentityCore<MedStockUser>(options =>
{
    options.Password.RequiredLength = 12;
    options.Password.RequireUppercase = true;
    options.Password.RequireDigit = true;
    options.Password.RequireNonAlphanumeric = true;

    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.AllowedForNewUsers = true;
    options.User.RequireUniqueEmail = true;
});


var identityBuilder = new IdentityBuilder(typeof(MedStockUser), typeof(IdentityRole), builder.Services);
identityBuilder.AddRoleValidator<RoleValidator<IdentityRole>>();
identityBuilder.AddRoleManager<RoleManager<IdentityRole>>();
identityBuilder.AddEntityFrameworkStores<ApplicationDbContext>();
identityBuilder.AddDefaultTokenProviders();
builder.Services.AddScoped<IPasswordHasher<MedStockUser>, BcryptPasswordHasher>();
builder.Services.AddScoped<IAuthorizationHandler, CanEditMedicineHandler>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanEditMedicine", policy =>
        policy.Requirements.Add(new CanEditMedicineRequirement()));
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("AuthLimiter", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        }));
});

builder.Services.AddProblemDetails();

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-XSRF-TOKEN";
});
var allowedOrigins = builder.Configuration
    .GetSection("AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:4200", "http://localhost:3000", "http://localhost:5173" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontendApp", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()
              .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
    });
});

builder.Services.AddControllers();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "MedStockTracker",
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "MedStockTrackerUsers",
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "SuperSecretSecureKeyThatIsLongEnoughAndProtectedSafely2026!"))
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (string.IsNullOrEmpty(context.Token) && context.Request.Cookies.ContainsKey("medstock_auth"))
            {
                context.Token = context.Request.Cookies["medstock_auth"];
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.ParameterLocation.Header,
        Description = "Enter your JWT token."
    });

    options.AddSecurityRequirement(document => new Microsoft.OpenApi.OpenApiSecurityRequirement
    {
        [new Microsoft.OpenApi.OpenApiSecuritySchemeReference("Bearer", document)] =
            new List<string>()
    });
});
var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value ?? string.Empty;

    // Scalar and Swagger UI require inline styles/scripts â€” relax CSP for doc routes only
    bool isDocRoute = app.Environment.IsDevelopment() &&
                      (path.StartsWith("/scalar", StringComparison.OrdinalIgnoreCase) ||
                       path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase) ||
                       path.StartsWith("/openapi", StringComparison.OrdinalIgnoreCase));

    context.Response.Headers["X-Content-Type-Options"] = "nosniff";

    if (isDocRoute)
    {
        // Scalar (Vue-based) needs unsafe-inline, unsafe-eval, external fonts, and api.scalar.com
        context.Response.Headers["Content-Security-Policy"] =
            "default-src 'self'; " +
            "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://cdn.jsdelivr.net; " +
            "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; " +
            "img-src 'self' data: https:; " +
            "font-src 'self' data: https://fonts.gstatic.com; " +
            "connect-src 'self' https://api.scalar.com;";
    }
    else
    {
        context.Response.Headers["X-Frame-Options"] = "DENY";
        context.Response.Headers["Content-Security-Policy"] = "default-src 'self'; frame-ancestors 'none';";
    }

    await next();
});
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); // serves /swagger/v1/swagger.json
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("MedStock Tracker System")
               .WithTheme(ScalarTheme.DeepSpace)
               .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
               .WithOpenApiRoutePattern("/swagger/v1/swagger.json");
    });
}

app.UseRouting();
app.UseCors("AllowFrontendApp");
app.UseRateLimiter();

app.Use(async (context, next) =>
{
    if (context.Request.Cookies.ContainsKey("medstock_auth") || context.User.Identity?.IsAuthenticated == true)
    {
        var antiforgery = context.RequestServices.GetRequiredService<IAntiforgery>();
        var tokens = antiforgery.GetAndStoreTokens(context);

        context.Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken!, new CookieOptions
        {
            HttpOnly = false,
            Secure = !app.Environment.IsDevelopment(),
            SameSite = SameSiteMode.Lax,
            Path = "/"
        });
    }

    await next();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        DataSeeder.SeedDataAsync(dbContext).GetAwaiter().GetResult();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred while running database seeding operations: {ex.Message}");
    }
}

app.Run();
