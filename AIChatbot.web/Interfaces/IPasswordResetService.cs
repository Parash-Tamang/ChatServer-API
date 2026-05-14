using AIChatbot.web.Dto;
using AIChatbot.web.Models.Admin;

namespace AIChatbot.web.Interfaces
{
    public interface IPasswordResetService
    {
        Task<ServiceResultDto> SendOtpAsync(ForgotPasswordDto dto);
        Task<ServiceResultDto> VerifyOtpAsync(VerifyOtpDto dto);
        Task<ServiceResultDto> ResetPasswordAsync(ResetPasswordDto dto);
    }
}
