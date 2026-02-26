using System.ComponentModel.DataAnnotations;

namespace AIChatbot.Api.Models.Auth;


/// Request to register a new user

public class RegisterRequest
{
    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Phone { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string Password { get; set; } = string.Empty;
    public required string Role { get; set; }
    
}
