using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;

using AIChatbot.Api.Middleware;
using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Chat.Validators;
using AIChatbot.Application.Common;
using AIChatbot.Application.Common.Behaviours;
using AIChatbot.Domain.Entities;
using AIChatbot.Infrastructure.AI;
using AIChatbot.Infrastructure.Data;
using AIChatbot.Infrastructure.Identity;
using AIChatbot.Infrastructure.Repositories;

using FluentValidation;
using MediatR;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ==========================================================
// ENVIRONMENT VARIABLES
// ==========================================================

builder.Configuration.AddEnvironmentVariables();

// ==========================================================
// REQUEST LIMITS
// ==========================================================

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize =
        5 * 1024 * 1024;
});

// ==========================================================
// CONTROLLERS + SWAGGER
// ==========================================================

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AI Core Platform API",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer",
        new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        });

    c.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference =
                        new OpenApiReference
                        {
                            Type =
                                ReferenceType.SecurityScheme,

                            Id = "Bearer"
                        }
                },

                Array.Empty<string>()
            }
        });
});

// ==========================================================
// MEDIATR + VALIDATION
// ==========================================================

builder.Services.AddMediatR(
    typeof(AIChatbot.Application.AssemblyReference)
        .Assembly);

builder.Services
    .AddValidatorsFromAssemblyContaining<
        SendChatMessageValidator>();

builder.Services.AddTransient(
    typeof(IPipelineBehavior<,>),
    typeof(ValidationBehavior<,>));

// ==========================================================
// DATABASE
// ==========================================================

builder.Services.AddDbContext<AppDbContext>(
    options =>
        options.UseSqlServer(
            builder.Configuration
                .GetConnectionString(
                    "DefaultConnection")));

// ==========================================================
// IDENTITY + PASSWORD + LOCKOUT
// ==========================================================

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(
        options =>
        {
            // PASSWORD

            options.Password.RequiredLength = 10;
            options.Password.RequireDigit = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = true;

            // LOCKOUT

            options.Lockout.MaxFailedAccessAttempts = 5;

            options.Lockout.DefaultLockoutTimeSpan =
                TimeSpan.FromMinutes(15);

            options.Lockout.AllowedForNewUsers = true;
        })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// ==========================================================
// JWT AUTHENTICATION
// ==========================================================

var jwt = builder.Configuration.GetSection("Jwt");

var jwtKey =
    Environment.GetEnvironmentVariable("JWT__KEY")
    ??
    jwt["Key"];

var key = Encoding.UTF8.GetBytes(jwtKey!);

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = jwt["Issuer"],

                ValidAudience = jwt["Audience"],

                IssuerSigningKey =
                    new SymmetricSecurityKey(key),

                NameClaimType =
                    ClaimTypes.NameIdentifier
            };

        options.Events =
            new JwtBearerEvents
            {
                OnChallenge = context =>
                {
                    context.HandleResponse();

                    context.Response.StatusCode = 401;

                    context.Response.ContentType =
                        "application/json";

                    return context.Response
                        .WriteAsJsonAsync(new
                        {
                            success = false,
                            message = "Unauthorized"
                        });
                }
            };
    });

// ==========================================================
// AUTHORIZATION
// ==========================================================

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        "SuperAdminOnly",
        policy =>
            policy.RequireRole("SuperAdmin"));

    options.AddPolicy(
        "AuthenticatedUser",
        policy =>
            policy.RequireAuthenticatedUser());
});

// ==========================================================
// RATE LIMITING
// ==========================================================

builder.Services.AddRateLimiter(options =>
{
    // GLOBAL LIMITER

    options.GlobalLimiter =
        PartitionedRateLimiter.Create<HttpContext, string>(
            context =>
            {
                var ip =
                    context.Connection.RemoteIpAddress?
                        .ToString()
                    ?? "unknown";

                return RateLimitPartition
                    .GetFixedWindowLimiter(
                        partitionKey: ip,

                        factory: _ =>
                            new FixedWindowRateLimiterOptions
                            {
                                PermitLimit = 100,

                                Window =
                                    TimeSpan.FromMinutes(1),

                                QueueLimit = 0
                            });
            });

    // LOGIN

    options.AddPolicy(
        "login",
        context =>
        {
            var ip =
                context.Connection.RemoteIpAddress?
                    .ToString()
                ?? "unknown";

            return RateLimitPartition
                .GetFixedWindowLimiter(
                    partitionKey: ip,

                    factory: _ =>
                        new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 500,

                            Window =
                                TimeSpan.FromMinutes(1),

                            QueueLimit = 0
                        });
        });

    // REGISTER

    options.AddPolicy(
        "register",
        context =>
        {
            var ip =
                context.Connection.RemoteIpAddress?
                    .ToString()
                ?? "unknown";

            return RateLimitPartition
                .GetFixedWindowLimiter(
                    partitionKey: ip,

                    factory: _ =>
                        new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 300,

                            Window =
                                TimeSpan.FromMinutes(1),

                            QueueLimit = 0
                        });
        });

    // CHAT

    options.AddPolicy(
        "chat",
        context =>
        {
            var user =
                context.User.Identity?.Name
                ??
                context.Connection.RemoteIpAddress?
                    .ToString()
                ??
                "unknown";

            return RateLimitPartition
                .GetFixedWindowLimiter(
                    partitionKey: user,

                    factory: _ =>
                        new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 200,

                            Window =
                                TimeSpan.FromMinutes(1),

                            QueueLimit = 0
                        });
        });

    // ROLES

    options.AddPolicy(
        "RolesPolicy",
        context =>
        {
            var ip =
                context.Connection.RemoteIpAddress?
                    .ToString()
                ?? "unknown";

            return RateLimitPartition
                .GetFixedWindowLimiter(
                    partitionKey: ip,

                    factory: _ =>
                        new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 200,

                            Window =
                                TimeSpan.FromMinutes(1),

                            QueueLimit = 0
                        });
        });

    options.OnRejected = async (
        context,
        token) =>
    {
        context.HttpContext.Response.StatusCode = 429;

        await context.HttpContext.Response
            .WriteAsJsonAsync(
                new
                {
                    success = false,
                    message = "Too many requests."
                },
                token);
    };
});

// ==========================================================
// CORS
// ==========================================================

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "Frontend",
        policy =>
        {
            policy
                .WithOrigins(
                    "http://localhost:3000",
                    "http://192.168.40.121:5198")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});

// ==========================================================
// SETTINGS
// ==========================================================

builder.Services.Configure<FrontendSettings>(
    builder.Configuration.GetSection(
        "Frontend"));

builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection(
        "EmailSettings"));

// ==========================================================
// REPOSITORIES + SERVICES
// ==========================================================

builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<
    IChatSessionRepository,
    ChatSessionRepository>();

builder.Services.AddScoped<
    IResponseMetadataRepository,
    ResponseMetadataRepository>();

builder.Services.AddScoped<
    IChatSessionNamingRepository,
    ChatSessionNamingRepository>();

builder.Services.AddScoped<
    IOtpRepository,
    OtpRepository>();

builder.Services.AddScoped<
    ILocalFunctionRepository,
    LocalFunctionRepository>();

builder.Services.AddScoped<
    IConnectionRepository,
    ConnectionRepository>();

builder.Services.AddScoped<
    IPromptFunctionRepository,
    PromptFunctionRepository>();
builder.Services.AddScoped<
    IRuntimeViewQueryBuilder,
    RuntimeViewQueryBuilder>();

builder.Services.AddScoped<
    IRuntimeViewService,
    RuntimeViewService>();

builder.Services.AddScoped<
    IRuntimePermissionViewRepository,
    RuntimePermissionViewRepository>();

builder.Services.AddScoped<
    IConnectionExclusionRepository,
    ConnectionExclusionRepository>();

builder.Services.AddScoped<
    IRoleInstructionRepository,
    RoleInstructionRepository>();

builder.Services.AddScoped<
    IRoleConnectionMappingRepository,
    RoleConnectionMappingRepository>();

builder.Services.AddScoped<
    IExternalUserLookupService,
    ExternalUserLookupService>();

builder.Services.AddScoped<
    IExternalUserMappingRepository,
    ExternalUserMappingRepository>();
builder.Services.AddScoped<
    IConnectionExclusionRepository,
    ConnectionExclusionRepository>();

builder.Services.AddScoped<
    IRoleConnectionUserLookupConfigurationRepository,
    RoleConnectionUserLookupConfigurationRepository>();

builder.Services.AddScoped<
    IRoleRuntimePermissionRepository,
    RoleRuntimePermissionRepository>();

builder.Services.AddScoped<
    IRuntimeContextResolver,
    RuntimeContextResolver>();

builder.Services.AddScoped<
    ISqlPermissionFilterBuilder,
    SqlPermissionFilterBuilder>();




// OTP

builder.Services.AddSingleton<
    IOtpStore,
    InMemoryOtpStore>();

builder.Services.AddScoped<
    IEmailService,
    EmailService>();

// ==========================================================
// AI PROVIDER
// ==========================================================

var aiBaseUrl =
    Environment.GetEnvironmentVariable(
        "AI__BASEURL")
    ??
    builder.Configuration["AI:BaseUrl"];

// ===== PRODUCTION =====

builder.Services.AddHttpClient<
    IAiProviderService,
    AiProviderService>(client =>
    {
        client.BaseAddress =
            new Uri(aiBaseUrl!);

        client.Timeout =
            TimeSpan.FromMinutes(5);
    });

// ===== TEST MODE =====

//builder.Services.AddHttpClient<
//    IAiProviderService,
//    ResponseProvider>(client =>
//{
//    client.BaseAddress =
//        new Uri("http://localhost:11434/");
//});

// ==========================================================
// BUILD
// ==========================================================

var app = builder.Build();

// ==========================================================
// SWAGGER
// ==========================================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ==========================================================
// SECURITY MIDDLEWARE
// ==========================================================

app.UseCors("Frontend");

app.UseMiddleware<SecurityHeadersMiddleware>();

// Uncomment after creating middleware
// app.UseMiddleware<InputSanitizationMiddleware>();

app.UseMiddleware<JsonEscapeFixMiddleware>();

app.UseMiddleware<ExceptionMappingMiddleware>();

app.UseRateLimiter();

// ==========================================================
// HTTPS
// ==========================================================

app.UseHttpsRedirection();

// ==========================================================
// AUTH
// ==========================================================

app.UseAuthentication();

app.UseAuthorization();

// ==========================================================
// MAP CONTROLLERS
// ==========================================================

app.MapControllers();

// ==========================================================
// SEEDING
// ==========================================================

using (var scope = app.Services.CreateScope())
{
    var roleManager =
        scope.ServiceProvider
            .GetRequiredService<
                RoleManager<IdentityRole>>();

    var userManager =
        scope.ServiceProvider
            .GetRequiredService<
                UserManager<ApplicationUser>>();

    var config =
        scope.ServiceProvider
            .GetRequiredService<IConfiguration>();

    await RoleSeeder.SeedRolesAsync(
        roleManager);

    await SuperAdminSeeder
        .SeedSuperAdminAsync(
            userManager,
            roleManager,
            config);
}

app.Run();