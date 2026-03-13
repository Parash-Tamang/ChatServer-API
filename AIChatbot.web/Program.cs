using AIChatbot.web.Filters;
using AIChatbot.web.Interfaces;
using AIChatbot.web.Models.Auth;
using AIChatbot.web.Services;
using AIChatbot.web.Validators;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddControllersWithViews();

// Core services
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();


// Custom services - Register interface
builder.Services.AddScoped<ApiClient>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<IValidator<LoginUser>,LoginValidator>();
builder.Services.AddScoped<IValidator<RegisterUser>, RegisterValidator>();
builder.Services.AddScoped<IAuthService>(sp => sp.GetRequiredService<AuthService>());
builder.Services.AddScoped<IRoleManagerService, RoleManagerService>();
builder.Services.AddScoped<ChatApiService>();
builder.Services.AddScoped<ITokenService,TokenService>();
builder.Services.AddScoped<TokenAuthorizationFilter>();


// Auth filter (for protecting dashboard/chat)
builder.Services.AddScoped<AuthFilter>();

builder.Services.AddScoped<IConnectionService, ConnectionService>();

var app = builder.Build();

// ---------------- PIPELINE ----------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=login}/{id?}");

app.Run();
