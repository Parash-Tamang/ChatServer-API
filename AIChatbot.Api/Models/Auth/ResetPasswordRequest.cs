using System.ComponentModel.DataAnnotations;

namespace AIChatbot.Api.Models.Auth;


/// Request to reset password using reset token

public class ResetPasswordRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Token { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string NewPassword { get; set; } = string.Empty;
}
