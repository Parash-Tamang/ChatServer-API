using System.ComponentModel.DataAnnotations;

namespace AIChatbot.web.Models.Auth
{
    public class ResetPasswordModel
    {
        [Required]
        public string Token { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Please confirm your password")]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }
    }
}