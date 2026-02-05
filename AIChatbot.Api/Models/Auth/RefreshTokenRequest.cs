using System.ComponentModel.DataAnnotations;

namespace AIChatbot.Api.Models.Auth;


/// Request to refresh JWT access token

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
