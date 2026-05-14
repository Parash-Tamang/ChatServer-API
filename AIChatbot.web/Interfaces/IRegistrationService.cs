using AIChatbot.web.Dto;

namespace AIChatbot.web.Interfaces
{
    public interface IRegistrationService
    {
        Task<ServiceResultDto> SendRegisterOtpAsync(SendRegisterOtpDto dto);
        Task<VerifyRegisterOtpResponseDto> VerifyRegisterOtpAsync(VerifyRegisterOtpDto dto);
    }
}