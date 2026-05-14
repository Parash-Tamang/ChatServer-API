using AIChatbot.web.Dto;
using AIChatbot.web.Interfaces;

namespace AIChatbot.web.Services
{
    public class PasswordResetService : IPasswordResetService
    {
        private readonly ApiClient _api;

        public PasswordResetService(ApiClient api)
        {
            _api = api;
        }

        public async Task<ServiceResultDto> SendOtpAsync(ForgotPasswordDto dto)
        {
            var res = await _api.PostAsync(
                "/api/auth/V1/Security-engine/forgot-password",
                dto);

            if (res == null)
                return new ServiceResultDto { Success = false, Error = "Server unreachable." };

            var data = await res.Content.ReadFromJsonAsync<ServiceResultDto>();

            if (!res.IsSuccessStatusCode)
                return new ServiceResultDto { Success = false, Error = data?.Error ?? "Failed to send OTP." };

            if (data == null)
                return new ServiceResultDto { Success = false, Error = "Invalid server response." };

            return data;
        }

        public async Task<ServiceResultDto> VerifyOtpAsync(VerifyOtpDto dto)
        {
            var res = await _api.PostAsync(
                "/api/auth/V1/Security-engine/verify-otp",
                dto);

            if (res == null)
                return new ServiceResultDto { Success = false, Error = "Server unreachable." };

            var data = await res.Content.ReadFromJsonAsync<ServiceResultDto>();

            if (!res.IsSuccessStatusCode)
                return new ServiceResultDto { Success = false, Error = data?.Error ?? "Invalid or expired OTP." };

            if (data == null)
                return new ServiceResultDto { Success = false, Error = "Invalid server response." };

            return data;
        }

        public async Task<ServiceResultDto> ResetPasswordAsync(ResetPasswordDto dto)
        {
            var res = await _api.PostAsync(
                "/api/auth/V1/Security-engine/reset-password",
                dto);

            if (res == null)
                return new ServiceResultDto { Success = false, Error = "Server unreachable." };

            var data = await res.Content.ReadFromJsonAsync<ServiceResultDto>();

            if (!res.IsSuccessStatusCode)
                return new ServiceResultDto { Success = false, Error = data?.Error ?? "Password reset failed." };

            if (data == null)
                return new ServiceResultDto { Success = false, Error = "Invalid server response." };

            return data;
        }
    }
}