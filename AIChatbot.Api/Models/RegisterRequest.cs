using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace AIChatbot.Api.Models;

public class RegisterRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string Password { get; set; } = string.Empty;
}
