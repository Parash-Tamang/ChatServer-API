using AIChatbot.web.Dto;
using AIChatbot.web.Interfaces;

namespace AIChatbot.web.Services
{
    public class RegistrationService : IRegistrationService
    {
        private readonly ApiClient _api;

        public RegistrationService(ApiClient api)
        {
            _api = api;
        }

        public async Task<ServiceResultDto> SendRegisterOtpAsync(SendRegisterOtpDto dto)
        {
            var res = await _api.PostAsync(
                "/api/auth/V1/Security-engine/send-register-otp", dto);

            if (res == null)
                return new ServiceResultDto { Success = false, Error = "Server unreachable." };

            var data = await res.Content.ReadFromJsonAsync<ServiceResultDto>();

            if (!res.IsSuccessStatusCode)
                return new ServiceResultDto { Success = false, Error = data?.Error ?? "Failed to send OTP." };

            if (data == null)
                return new ServiceResultDto { Success = false, Error = "Invalid server response." };

            return data;
        }

        public async Task<VerifyRegisterOtpResponseDto> VerifyRegisterOtpAsync(VerifyRegisterOtpDto dto)
        {
            var res = await _api.PostAsync(
                "/api/auth/V1/Security-engine/verify-register-otp", dto);

            if (res == null)
                return new VerifyRegisterOtpResponseDto { Success = false, Error = "Server unreachable." };

            var data = await res.Content.ReadFromJsonAsync<VerifyRegisterOtpResponseDto>();

            if (!res.IsSuccessStatusCode)
                return new VerifyRegisterOtpResponseDto { Success = false, Error = data?.Error ?? "Invalid or expired OTP." };

            if (data == null)
                return new VerifyRegisterOtpResponseDto { Success = false, Error = "Invalid server response." };

            return data;
        }
    }
}