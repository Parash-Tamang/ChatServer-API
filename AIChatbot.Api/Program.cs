using AIChatbot.Api.Middleware;
using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Chat.Handlers;
using AIChatbot.Application.Chat.Validators;
using AIChatbot.Application.Common.Behaviours;

using AIChatbot.Application.RoleAccess.Services;
using AIChatbot.Application.RoleManagement.Handlers;
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

// -------------------- Controllers & Swagger --------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new()
    {
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"

    });
  
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "AI Core Platform API",
            Version = "v1"
        });
 


    c.AddSecurityRequirement(new()
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new()
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
// --------------------Network --------------------
//builder.WebHost.UseUrls(
//    "http://192.168.10.96:5048",
//    "https://192.168.10.96:7048"
//);
// -------------------- MediatR + Validation --------------------
builder.Services.AddMediatR(typeof(AIChatbot.Application.AssemblyReference).Assembly);
builder.Services.AddValidatorsFromAssemblyContaining<SendChatMessageValidator>();
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// -------------------- Database --------------------
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// -------------------- Identity --------------------
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// -------------------- JWT Authentication --------------------
var jwt = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwt["Key"]!);

builder.Services.AddAuthentication(options =>
{
    // 🔥 FORCE JWT ONLY
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwt = builder.Configuration.GetSection("Jwt");
    var key = Encoding.UTF8.GetBytes(jwt["Key"]!);

    options.TokenValidationParameters = new()
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
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = ctx =>
    {
        ctx.Response.StatusCode = 401;
        return Task.CompletedTask;
    };

    options.Events.OnRedirectToAccessDenied = ctx =>
    {
        ctx.Response.StatusCode = 403;
        return Task.CompletedTask;
    };
});

builder.Services.AddAuthorization();

// -------------------- Dependency Injection --------------------
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IChatSessionRepository, ChatSessionRepository>();
builder.Services.AddScoped<IConnectionRepository, ConnectionRepository>();
builder.Services.AddScoped<IPromptRepository, PromptRepository>();
builder.Services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();
builder.Services.AddScoped<IRoleAccessService, RoleAccessService>();
builder.Services.AddScoped<ISchemaRepository, SchemaRepository>();
builder.Services.AddScoped<SchemaAccessService>();
//builder.Services.AddHttpClient<IAiProviderService, AiProviderService>();
//builder.Services.AddHttpClient<IAiProviderService, AiProviderService>(client =>
//{
//    client.BaseAddress = new Uri("http://localhost:5000/");
//    client.Timeout = TimeSpan.FromMinutes(5);
//});

builder.Services.AddHttpClient<IAiProviderService, ResponseProvider>();
// add for testing without Flask
builder.Services.AddScoped<IResponseMetadataRepository, ResponseMetadataRepository>();

builder.Services.AddScoped<IChatSessionNamingRepository, ChatSessionNamingRepository>();



var app = builder.Build();

// -------------------- HTTP Pipeline --------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

//  Global exception handler (must be here)
app.UseMiddleware<ExceptionMappingMiddleware>();

app.MapControllers();

// Health check
app.MapGet("/ping", () => "pong");

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

    await RoleSeeder.SeedRolesAsync(roleManager);
    await SuperAdminSeeder.SeedSuperAdminAsync(userManager, roleManager, config);
}

app.Run();
