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
            options.Password.RequireNonAlphanumeric = true; // special character
            options.Password.RequiredUniqueChars = 1;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        // 3. Read JWT settings from appsettings.json
        var jwtSettings = new JwtSettings();
        config.Bind(nameof(JwtSettings), jwtSettings);
        services.AddSingleton(jwtSettings);

        // 4. Register JWT Authentication Scheme
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
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

            // ✅ CUSTOM AUTH RESPONSES
            options.Events = new JwtBearerEvents
            {
                // ❌ Token invalid / expired
                OnAuthenticationFailed = context =>
                {
                    context.NoResult();
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

                    return context.Response.WriteAsJsonAsync(response);
                },

                // ❌ Token missing
                OnChallenge = context =>
                {
                    context.HandleResponse(); // VERY IMPORTANT
                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "application/json";

                    var response = new
                    {
                        success = false,
                        error = "TOKEN_MISSING",
                        message = "Access token is missing"
                    };

                    return context.Response.WriteAsJsonAsync(response);
                },

                // ❌ Authenticated but not authorized (roles)
                OnForbidden = context =>
                {
                    context.Response.StatusCode = 403;
                    context.Response.ContentType = "application/json";

                    var response = new
                    {
                        success = false,
                        error = "FORBIDDEN",
                        message = "You do not have permission to access this resource"
                    };

                    return context.Response.WriteAsJsonAsync(response);
                }
            };
        });

        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IChatSessionService, ChatSessionService>();
        services.AddScoped<IChatMessageService, ChatMessageService>();
        services.AddScoped<IAgentService, AgentService>();
        services.AddScoped<ISchoolService, SchoolService>();
    
        return services;
    }
}
