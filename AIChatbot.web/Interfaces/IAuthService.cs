using AIChatbot.web.Models.Auth;

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
        Task<AuthResponse> LoginAsync(LoginRequest req);

        /// <summary>
        /// Registers a new user and issues tokens
        /// </summary>
        Task<AuthResponse> RegisterAsync(RegisterRequest req);

        /// <summary>
        /// Issues a new access token using refresh token
        /// </summary>
        Task<AuthResponse> RefreshTokenAsync(string refreshToken);

        /// <summary>
        /// Logs out the user by revoking active tokens
        /// </summary>
        Task LogoutAsync();

        /// <summary>
        /// Get user Details
        /// </summary>
        Task<string> GetUserDetailsAsync();

        /// <summary>
        /// Validates login input
        /// </summary>
        (bool isValid, string errorMessage) ValidateLoginInput(LoginRequest req);

        /// <summary>
        /// Validates registration input with multiple field validation
        /// </summary>
        (bool isValid, Dictionary<string, string> errors) ValidateRegisterInput(RegisterRequest req);
    }
}
