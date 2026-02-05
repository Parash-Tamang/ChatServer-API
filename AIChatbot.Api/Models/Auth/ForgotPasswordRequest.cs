using System.ComponentModel.DataAnnotations;

namespace AIChatbot.Api.Models.Auth;


/// Request to initiate forgot-password flow

public class ForgotPasswordRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}
