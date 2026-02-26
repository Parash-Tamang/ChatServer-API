using AIChatbot.Application.Auth.Results;

namespace AIChatbot.Application.Abstractions;

/// Authentication and authorization service
/// Handles login, registration, tokens, and password flows

public interface IAuthService
{
  
    /// Authenticates a user and issues access & refresh tokens
 
    Task<AuthResult> LoginAsync(string email, string password);

    /// Registers a new user and issues tokens
   
    Task<AuthResult> RegisterAsync(
        string firstName,
        string lastName,
        string email,
        string phone,
        string password,
        string Role);

   
    /// Issues a new access token using a refresh token
 
    Task<AuthResult> RefreshTokenAsync(string refreshToken);

 
    /// Logs out the user by revoking active tokens
   
    Task LogoutAsync(string userId);

    /// Get user Details

    Task<UserProfileResult> GetProfileAsync(string userId);


    /// Initiates forgot-password flow

    Task ForgotPasswordAsync(string email);

   
    /// Resets user password using reset token
   
    Task ResetPasswordAsync(string email, string token, string newPassword);

   
    /// Revokes all tokens/sessions for the given user
  
    Task RevokeAllAsync(string userId);
    

}
