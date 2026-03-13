using AIChatbot.web.Models.Auth;
using AIChatbot.web.Dto;
namespace AIChatbot.web.Interfaces
{
    /// <summary>
    /// Authentication and authorization service
    /// Handles login, registration, tokens, and password flows
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Authenticates a user and issues access & refresh tokens
        /// </summary>
        Task<AuthResponseDto> LoginAsync(LoginUserDto loginUserDto );

        /// <summary>
        /// Registers a new user and issues tokens
        /// </summary>
        Task<AuthResponseDto> RegisterAsync(RegisterUserDto registerUserDto);

        /// <summary>
        /// Issues a new access token using refresh token
        /// </summary>
        Task<AuthResponse> RefreshTokenAsync(string refreshToken);

        /// <summary>
        /// Logs out the user by revoking active tokens
        /// </summary>
        Task LogoutAsync();

    }
}
