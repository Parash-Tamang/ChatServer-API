using System.ComponentModel.DataAnnotations;

namespace AIChatbot.web.Models.Auth
{
    public class VerifyOtpModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "OTP is required")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must be 6 digits")]
        public string Otp { get; set; }
    }
}