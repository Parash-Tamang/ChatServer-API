using Agent.Application.Dto.UserManagement;
using Agent.Application.Interface;
using Agent.Domain.Entities.UserManagement;
using Agent.Infrastructure.Data;
using Agent.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
namespace Agent.Infrastructure.Services
{
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TokenService(
            JwtSettings jwtSettings,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _jwtSettings = jwtSettings;
            _context = context;
            _userManager = userManager;
        }

        public async Task<JwtTokenResponseDto> GenerateTokenAsync(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim("fullName", user.UserName),
            new Claim(JwtRegisteredClaimNames.Email, user.Email)
        };

            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expireAt = DateTime.Now.AddMinutes(_jwtSettings.DurationInMinutes);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: expireAt,
                signingCredentials: creds);

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

            return new JwtTokenResponseDto { AccessToken = accessToken, ExpiresAt = expireAt };

        }

        public async Task<string> GenerateRefreshTokenAsync(string userId)
        {
            // 1. Generate secure random token
            var token = Convert.ToBase64String(
                RandomNumberGenerator.GetBytes(64));


            var expireAt = DateTime.Now.AddDays(
                    _jwtSettings.RefreshTokenValidityInDays);
            // 2. Create refresh token entity
            var refreshToken = new RefreshToken
            {
                UserId = userId,
                Token = token,
                ExpiryDate = expireAt,
                IsUsed = false,
                IsRevoked = false
            };

            // 3. Save to database
            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            // 4. Return token string
            return token;
        }

        public async  Task<RefreshToken> ValidateAndConsumeRefreshTokenAsync(string refreshToken)
        {
            var storedToken = await _context.RefreshTokens.FirstOrDefaultAsync(x => x.Token == refreshToken);

            // 1️⃣ Validate
            if (storedToken == null)
                throw new SecurityTokenException("Refresh token not found");

            if (storedToken.IsUsed)
                throw new SecurityTokenException("Refresh token already used");

            if (storedToken.IsRevoked)
                throw new SecurityTokenException("Refresh token revoked");

            if (storedToken.ExpiryDate < DateTime.Now)
                throw new SecurityTokenException("Refresh token expired");
            
            // 2️⃣ Consume (rotate)

            //storedToken.IsUsed = true;

            _context.RefreshTokens.Update(storedToken);
            await _context.SaveChangesAsync();

            return storedToken;

        }

        public async Task RevokeAllRefreshTokenAsync(string userId)
        {
            var tokens = await _context.RefreshTokens.Where(x => x.UserId == userId && !x.IsRevoked).ToListAsync();

            foreach (var token in tokens)
            {
                token.IsRevoked = true;
            }
            await _context.SaveChangesAsync();
        }

        public async Task RevokeAsync(string refreshToken)
        {
            var Token = await _context.RefreshTokens.FirstOrDefaultAsync(x => x.Token == refreshToken);

            if (Token != null)
            {
                Token.IsRevoked = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}
