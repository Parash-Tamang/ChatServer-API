using AIChatbot.Api.Middleware;
using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Chat.Validators;
using AIChatbot.Application.Common.Behaviours;
using AIChatbot.Application.RoleAccess.Services;
using AIChatbot.Domain.Entities;
using AIChatbot.Infrastructure.AI;
using AIChatbot.Infrastructure.Data;
using AIChatbot.Infrastructure.Identity;
using AIChatbot.Infrastructure.Repositories;

using FluentValidation;
using MediatR;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


// ==========================================================
// Controllers + Swagger
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

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});


// ==========================================================
// MediatR + Validation
// ==========================================================
builder.Services.AddMediatR(
    typeof(AIChatbot.Application.AssemblyReference).Assembly);

builder.Services.AddValidatorsFromAssemblyContaining<SendChatMessageValidator>();

builder.Services.AddTransient(typeof(IPipelineBehavior<,>),
    typeof(ValidationBehavior<,>));


// ==========================================================
// Database
// ==========================================================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));


// ==========================================================
// Identity
// ==========================================================
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();


// ==========================================================
// JWT Authentication
// ==========================================================
var jwt = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwt["Key"]!);

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
        ValidIssuer = jwt["Issuer"],
        ValidAudience = jwt["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        NameClaimType = ClaimTypes.NameIdentifier
    };

    options.Events = new JwtBearerEvents
    {
        OnChallenge = context =>
        {
            context.HandleResponse();
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";

            return context.Response.WriteAsJsonAsync(new
            {
                error = "Unauthorized - token missing or invalid"
            });
        }
    };
});

builder.Services.AddAuthorization();


// ==========================================================
// Dependency Injection
// ==========================================================
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IChatSessionRepository, ChatSessionRepository>();
builder.Services.AddScoped<IResponseMetadataRepository, ResponseMetadataRepository>();
builder.Services.AddScoped<IChatSessionNamingRepository, ChatSessionNamingRepository>();

builder.Services.AddScoped<IConnectionRepository, ConnectionRepository>();
builder.Services.AddScoped<IPromptFunctionRepository, PromptFunctionRepository>();

builder.Services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();
builder.Services.AddScoped<IRoleAccessService, RoleAccessService>();


// ==========================================================
// 🔥 AI PROVIDER SWITCH
// ==========================================================

// ===== PRODUCTION (Flask) =====
//builder.Services.AddHttpClient<IAiProviderService, AiProviderService>(client =>
//{
//    client.BaseAddress = new Uri("http://localhost:8000/");
//    client.Timeout = TimeSpan.FromMinutes(5);
//});
/// ==== Python connect vai wifi hotspot, use this if Flask is running on a different machine =====
builder.Services.AddHttpClient<IAiProviderService, AiProviderService>(client =>
{
    client.BaseAddress = new Uri("http://192.168.40.120:5000/");
    client.Timeout = TimeSpan.FromMinutes(5);
});
// ===== TEST MODE (Uncomment if needed) =====
//builder.Services.AddHttpClient<IAiProviderService, ResponseProvider>(client =>
//{
//    client.BaseAddress = new Uri("http://localhost:11434/");
//});


// ==========================================================
// Build App
// ==========================================================
var app = builder.Build();


// ==========================================================
// Middleware Pipeline
// ==========================================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<JsonEscapeFixMiddleware>();
app.UseMiddleware<ExceptionMappingMiddleware>();

app.MapControllers();

app.MapGet("/ping", () => "pong");
////====== get roles in register page =======
app.MapGet("/List all roles", (RoleManager<IdentityRole> roleManager) =>
{
    var roles = roleManager.Roles
        .Where(r => r.Name != "SuperAdmin" && r.Name != "Admin")
        .Select(r => r.Name)
        .ToList();

    var rolesString = string.Join(",", roles);

    return Results.Ok(new
    {
        success = true,
        roles = roles
    });
});

// ==========================================================
// Seed Roles + SuperAdmin
// ==========================================================
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

    await RoleSeeder.SeedRolesAsync(roleManager);
    await SuperAdminSeeder.SeedSuperAdminAsync(userManager, roleManager, config);
}

app.Run();