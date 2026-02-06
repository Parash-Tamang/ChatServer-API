using Agent.Domain.Entities.UserManagement;
using Agent.Infrastructure.Data;
using Agent.Infrastructure.Identity;
using Agent.Infrastructure.Services;
using Agent.Application.Interface;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace Agent.Infrastructure.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration config)
    {
        // 1. Register DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                config.GetConnectionString("DefaultConnection")));

        // 2. Register Identity
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequiredLength = 8;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireDigit = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredUniqueChars = 1;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        // 3. Read JWT settings
        var jwtSettings = new JwtSettings();
        config.Bind(nameof(JwtSettings), jwtSettings);
        services.AddSingleton(jwtSettings);

        // 4. JWT Authentication
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            var key = Encoding.UTF8.GetBytes(jwtSettings.Key);

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,

                ValidateAudience = true,
                ValidAudience = jwtSettings.Audience,

                ValidateLifetime = true,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),

                ClockSkew = TimeSpan.Zero
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = async context =>
                {
                    // ⚠️ Must set NoResult() to prevent default response
                    context.NoResult();

                    // ⚠️ Check if response hasn't started
                    if (!context.Response.HasStarted)
                    {
                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json";

                        var response = new
                        {
                            success = false,
                            error = context.Exception is SecurityTokenExpiredException
                                ? "TOKEN_EXPIRED"
                                : "TOKEN_INVALID",
                            message = context.Exception is SecurityTokenExpiredException
                                ? "Access token expired"
                                : "Invalid access token"
                        };

                        await context.Response.WriteAsJsonAsync(response);
                    }
                },

                OnChallenge = async context =>
                {
                    // ✅ CRITICAL: Must call this FIRST to prevent default response
                    context.HandleResponse();

                    // ⚠️ Check if response hasn't started
                    if (!context.Response.HasStarted)
                    {
                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json";

                        var response = new
                        {
                            success = false,
                            error = "TOKEN_MISSING",
                            message = "Access token is missing"
                        };

                        await context.Response.WriteAsJsonAsync(response);
                    }
                },

                OnForbidden = async context =>
                {
                    // ⚠️ Check if response hasn't started
                    if (!context.Response.HasStarted)
                    {
                        context.Response.StatusCode = 403;
                        context.Response.ContentType = "application/json";

                        var response = new
                        {
                            success = false,
                            error = "FORBIDDEN",
                            message = "You do not have permission to access this resource"
                        };

                        await context.Response.WriteAsJsonAsync(response);
                    }
                }
            };
        });

        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IChatSessionService, ChatSessionService>();
        services.AddScoped<IChatMessageService, ChatMessageService>();
        services.AddHttpClient<IAgentService, AgentService>();
        services.AddScoped<ISchoolService, SchoolService>();

        return services;
    }
}