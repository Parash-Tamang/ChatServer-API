using AIChatbot.Application.DTOs;
using AIChatbot.Application.Interfaces;
using AIChatbot.Infrastructure.Data;
using AIChatbot.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AIChatbot.Domain.Entities;
using Microsoft.AspNetCore.Components.Forms;

namespace AIChatbot.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        AppDbContext context,
        IConfiguration config)
    {
        _userManager = userManager;
        _context = context;
        _config = config;
    }

    // =========================
    // REGISTER
    // =========================
    public async Task<AuthResult> RegisterAsync(string email, string password)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
         
        };

        var result = await _userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            return new AuthResult
            {
                Success = false,
                Error = string.Join(", ", result.Errors.Select(e => e.Description))
            };
        }

        return await GenerateAuthResultAsync(user);
    }

    // =========================
    // LOGIN
    // =========================
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

        return await GenerateAuthResultAsync(user);
    }

    // =========================
    // REFRESH TOKEN
    // =========================
    public async Task<AuthResult> RefreshTokenAsync(string refreshToken)
    {
        var hashed = Hash(refreshToken);

        var token = await _context.Set<RefreshToken>()
            .FirstOrDefaultAsync(x =>
                x.TokenHash == hashed &&
                !x.IsRevoked &&
                x.ExpiresAt > DateTime.UtcNow);

        if (token == null)
        {
            return new AuthResult
            {
                Success = false,
                Error = "Invalid refresh token"
            };
        }

        token.IsRevoked = true;
        await _context.SaveChangesAsync();

        var user = await _userManager.FindByIdAsync(token.UserId);
        if (user == null)
        {
            return new AuthResult
            {
                Success = false,
                Error = "User not found"
            };
        }

        return await GenerateAuthResultAsync(user);
    }

    // =========================
    // LOGOUT (revoke active refresh tokens)
    // =========================
    public async Task LogoutAsync(string userId)
    {
        var tokens = await _context.RefreshTokens
            .Where(x =>
                x.UserId == userId &&
                !x.IsRevoked &&
                x.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();

        if (!tokens.Any())
            return;

        foreach (var token in tokens)
            token.IsRevoked = true;

        await _context.SaveChangesAsync();
    }

    // =========================
    // REVOKE ALL TOKENS (force logout everywhere)
    // =========================
    public async Task RevokeAllAsync(string userId)
    {
        var tokens = await _context.RefreshTokens
            .Where(x => x.UserId == userId)
            .ToListAsync();

        if (!tokens.Any())
            return;

        foreach (var token in tokens)
            token.IsRevoked = true;

        await _context.SaveChangesAsync();
    }

    // =========================
    // TOKEN GENERATION
    // =========================
    private async Task<AuthResult> GenerateAuthResultAsync(ApplicationUser user)
    {
        var accessToken = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken();

        var refreshEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = Hash(refreshToken),
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        _context.Add(refreshEntity);
        await _context.SaveChangesAsync();

        return new AuthResult
        {
            Success = true,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = 10800 // 180 minutes
        };
    }

    private string GenerateAccessToken(ApplicationUser user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Key"]!)
        );

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    private static string Hash(string input)
    {
        using var sha = SHA256.Create();
        return Convert.ToBase64String(
            sha.ComputeHash(Encoding.UTF8.GetBytes(input))
        );
    }
}
