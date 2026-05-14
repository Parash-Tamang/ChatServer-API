using System.ComponentModel.DataAnnotations;

namespace AIChatbot.web.Models.Auth
{
    public class ForgotPasswordModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }
    }
}