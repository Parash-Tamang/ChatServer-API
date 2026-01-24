using AIChatbot.Application.DTOs;

namespace AIChatbot.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResult> LoginAsync(string email, string password);
    Task<AuthResult> RegisterAsync(string email, string password);
    Task<AuthResult> RefreshTokenAsync(string refreshToken);

    Task LogoutAsync(string userId);
    Task RevokeAllAsync(string userId);
}
