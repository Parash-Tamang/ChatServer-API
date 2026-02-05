using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Auth.Results;
using AIChatbot.Domain.Entities;
using AIChatbot.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AIChatbot.Infrastructure.Identity;

/// <summary>
/// Authentication and authorization service
/// Handles JWT, refresh tokens, login, register, logout, and password flows
/// </summary>
public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        AppDbContext db,
        IConfiguration config)
    {
        _userManager = userManager;
        _db = db;
        _config = config;
    }

    // ---------------- REGISTER ----------------
    public async Task<AuthResult> RegisterAsync(
        string firstName,
        string lastName,
        string email,
        string phone,
        string password)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            PhoneNumber = phone,
            FirstName = firstName,
            LastName = lastName
        };

        var result = await _userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            return new AuthResult
            {
                Success = false,
                Error = string.Join("; ", result.Errors.Select(e => e.Description))
            };
        }

        return await IssueAuthResultAsync(user);
    }

    // ---------------- LOGIN ----------------
    public async Task<AuthResult> LoginAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null || !await _userManager.CheckPasswordAsync(user, password))
        {
            return new AuthResult
            {
                Success = false,
                Error = "Invalid credentials"
            };
        }

        return await IssueAuthResultAsync(user);
    }

    // ---------------- REFRESH TOKEN ----------------
    public async Task<AuthResult> RefreshTokenAsync(string refreshToken)
    {
        var hash = Hash(refreshToken);

        var storedToken = await _db.RefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == hash);

        if (storedToken == null ||
            storedToken.IsRevoked ||
            storedToken.ExpiresAt < DateTime.UtcNow)
        {
            if (storedToken != null)
                await RevokeAllAsync(storedToken.UserId);

            return new AuthResult
            {
                Success = false,
                Error = "Invalid refresh token"
            };
        }

        // Rotate token
        storedToken.IsRevoked = true;

        var user = await _userManager.FindByIdAsync(storedToken.UserId)
                   ?? throw new KeyNotFoundException("User not found");

        return await IssueAuthResultAsync(user);
    }

    // ---------------- LOGOUT ----------------
    public async Task LogoutAsync(string userId)
        => await RevokeAllAsync(userId);

    public async Task RevokeAllAsync(string userId)
    {
        var tokens = _db.RefreshTokens
            .Where(t => t.UserId == userId && !t.IsRevoked);

        foreach (var token in tokens)
            token.IsRevoked = true;

        await _db.SaveChangesAsync();
    }

    // ---------------- PASSWORD RESET ----------------
    public async Task ForgotPasswordAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return;

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        // 🔔 Send token via email/SMS (implementation external)
    }

    public async Task ResetPasswordAsync(
        string email,
        string token,
        string newPassword)
    {
        var user = await _userManager.FindByEmailAsync(email)
                   ?? throw new KeyNotFoundException("User not found");

        var result = await _userManager.ResetPasswordAsync(
            user, token, newPassword);

        if (!result.Succeeded)
            throw new InvalidOperationException("Password reset failed");
    }

    // ---------------- TOKEN CORE ----------------
    private async Task<AuthResult> IssueAuthResultAsync(ApplicationUser user)
    {
        var accessToken = GenerateJwt(user);
        var refreshToken = GenerateSecureToken();

        var refreshDays =
            int.Parse(_config["Jwt:RefreshTokenExpirationDays"] ?? "7");

        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = Hash(refreshToken),
            ExpiresAt = DateTime.UtcNow.AddDays(refreshDays)
        });

        await _db.SaveChangesAsync();

        return new AuthResult
        {
            Success = true,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn =
                int.Parse(_config["Jwt:AccessTokenExpirationMinutes"] ?? "60") * 60
        };
    }

    // ---------------- JWT GENERATION ----------------
    private string GenerateJwt(ApplicationUser user)
    {
        var jwt = _config.GetSection("Jwt");

        var claims = new List<Claim>
        {
            // 🔑 REQUIRED: makes [Authorize] + Chat APIs work
            new Claim(ClaimTypes.NameIdentifier, user.Id),

            // Standard JWT claims
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwt["Key"]!));

        var creds = new SigningCredentials(
            key, SecurityAlgorithms.HmacSha256);

        var expires = DateTime.UtcNow.AddMinutes(
            int.Parse(jwt["AccessTokenExpirationMinutes"] ?? "60"));

        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // ---------------- HELPERS ----------------
    private static string Hash(string input)
    {
        using var sha = SHA256.Create();
        return Convert.ToBase64String(
            sha.ComputeHash(Encoding.UTF8.GetBytes(input)));
    }

    private static string GenerateSecureToken()
        => Convert.ToBase64String(
            RandomNumberGenerator.GetBytes(64));
}
