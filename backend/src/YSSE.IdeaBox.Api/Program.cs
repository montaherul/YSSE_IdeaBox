using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using YSSE.IdeaBox.Api.Common;
using YSSE.IdeaBox.Api.Health;
using YSSE.IdeaBox.Api.Middleware;
using YSSE.IdeaBox.Api.Hubs;
using YSSE.IdeaBox.Api.Security;
using YSSE.IdeaBox.Api.Services;
using YSSE.IdeaBox.Application;
using YSSE.IdeaBox.Application.Abstractions;
using YSSE.IdeaBox.Infrastructure;
using YSSE.IdeaBox.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------------------ logging
builder.Host.UseSerilog((_, loggerConfiguration) => loggerConfiguration
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console());

// ------------------------------------------------------------------ hosting limits
const long MaxUploadBytes = 15 * 1024 * 1024;
builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = MaxUploadBytes);
builder.Services.Configure<FormOptions>(options => options.MultipartBodyLengthLimit = MaxUploadBytes);
builder.Services.Configure<HostOptions>(options => options.ShutdownTimeout = TimeSpan.FromSeconds(30));

// ------------------------------------------------------------------ layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Real-time notifications. Uses the in-process SignalR backplane by default; when a
// Redis connection string is configured it also wires a Redis cache/backplane so a
// multi-instance deployment stays consistent.
var signalRBuilder = builder.Services.AddSignalR();
builder.Services.AddScoped<IRealtimeNotifier, SignalRRealtimeNotifier>();

var redisConnectionString = builder.Configuration["Redis:ConnectionString"];
if (!string.IsNullOrWhiteSpace(redisConnectionString))
{
    builder.Services.AddStackExchangeRedisCache(options => options.Configuration = redisConnectionString);
    signalRBuilder.AddStackExchangeRedis(redisConnectionString);
}

// ------------------------------------------------------------------ controllers + validation envelope
builder.Services.AddScoped<FluentValidationFilter>();
builder.Services
    .AddControllers(options => options.Filters.AddService<FluentValidationFilter>())
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var details = context.ModelState
            .Where(entry => entry.Value?.Errors.Count > 0)
            .ToDictionary(
                entry => entry.Key,
                entry => entry.Value!.Errors.Select(error => error.ErrorMessage).ToArray());

        return new BadRequestObjectResult(new ApiErrorResponse
        {
            Success = false,
            TraceId = context.HttpContext.TraceIdentifier,
            Error = new ApiError
            {
                Message = "One or more validation errors occurred.",
                Code = "VALIDATION",
                Details = details
            }
        });
    };
});

// ------------------------------------------------------------------ swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "YSSE Idea Box API", Version = "v1" });

    var scheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter 'Bearer {token}'",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };

    options.AddSecurityDefinition("Bearer", scheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement { [scheme] = Array.Empty<string>() });
});

// ------------------------------------------------------------------ authentication
var isDevelopment = builder.Environment.IsDevelopment();
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new InvalidOperationException(
        "Jwt:Key is not configured. Provide a strong signing key through configuration, " +
        "user-secrets, or environment variables (Jwt__Key).");
}

if (!isDevelopment && jwtKey.Length < 32)
{
    throw new InvalidOperationException("Jwt:Key must be at least 32 characters outside Development.");
}

var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "YSSE.IdeaBox";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "YSSE.IdeaBox";

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            NameClaimType = "name",
            RoleClaimType = "role"
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("Administrator"));
    options.AddPolicy("Moderator", policy => policy.RequireRole("Moderator", "Administrator"));
    options.AddPolicy("ScreeningReviewer", policy => policy.RequireRole("ScreeningReviewer", "Administrator"));
    options.AddPolicy("OpportunitySeeker", policy => policy.RequireRole("Entrepreneur", "Investor", "Mentor"));
});

// Permission-based policies ("users.read", "roles.manage", …) are produced on demand
// from the Permissions catalog and enforced by the permission handler.
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

// ------------------------------------------------------------------ cors
var allowedOrigins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>()
    ?? new[] { "http://localhost:5173" };

builder.Services.AddCors(options => options.AddPolicy("spa", policy => policy
    .WithOrigins(allowedOrigins)
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()));

// ------------------------------------------------------------------ rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.ContentType = "application/json";
        var payload = ApiResults.Error(
            "Too many requests. Please slow down and try again shortly.",
            "RATE_LIMITED",
            context.HttpContext.TraceIdentifier);
        await context.HttpContext.Response.WriteAsync(
            System.Text.Json.JsonSerializer.Serialize(payload,
                new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web)), token);
    };

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        var key = httpContext.User?.Identity?.IsAuthenticated == true
            ? httpContext.User.FindFirstValue("sub") ?? "anonymous"
            : httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous";

        return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 120,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        });
    });

    options.AddFixedWindowLimiter("auth", limiter =>
    {
        limiter.PermitLimit = 10;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueLimit = 0;
    });

    options.AddFixedWindowLimiter("guest-review", limiter =>
    {
        limiter.PermitLimit = 3;
        limiter.Window = TimeSpan.FromHours(1);
        limiter.QueueLimit = 0;
    });

    options.AddFixedWindowLimiter("write", limiter =>
    {
        limiter.PermitLimit = 30;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueLimit = 0;
    });

    options.AddFixedWindowLimiter("messaging", limiter =>
    {
        limiter.PermitLimit = 60;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueLimit = 0;
    });
});

// ------------------------------------------------------------------ request timeouts
builder.Services.AddRequestTimeouts(options => options.DefaultPolicy =
    new Microsoft.AspNetCore.Http.Timeouts.RequestTimeoutPolicy { Timeout = TimeSpan.FromSeconds(30) });

// ------------------------------------------------------------------ health checks
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database", tags: new[] { "ready" });

var app = builder.Build();

// ------------------------------------------------------------------ pipeline
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}

app.UseCors("spa");
app.UseRateLimiter();
app.UseRequestTimeouts();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");
app.MapGet("/", () => Results.Json(new { name = "YSSE Idea Box API", status = "ok", version = "v1" }));
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false
});
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

// ------------------------------------------------------------------ migrate + seed
await app.Services.InitializeAsync(isDevelopment);

app.Run();
