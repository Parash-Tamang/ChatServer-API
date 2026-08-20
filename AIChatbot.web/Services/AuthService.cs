using AIChatbot.web.Models.Auth;
using AIChatbot.web.Interfaces;
using System.Text.RegularExpressions;
using AIChatbot.web.Dto;

namespace AIChatbot.web.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApiClient _api;
        private readonly ITokenService _token;

        public AuthService(ApiClient api, ITokenService token)
        {
            _api = api;
            _token = token;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginUserDto req)
        {
            var res = await _api.PostAsync(
                "/api/auth/V1/Security-engine/login",
                req);

            if (res == null)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Error = "Authentication server is unreachable."
                };
            }
            var data = await res.Content.ReadFromJsonAsync<AuthResponseDto>();

            // Transport error
            if (!res.IsSuccessStatusCode)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Error = data?.Error ?? "Login failed"
                };
            }
            // Invalid API response
            if (data == null)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Error = "Invalid server response"
                };
            }

            // Business logic error
            if (!data.Success)
            {
                return data;
            }

            // Save tokens
            if (data.AccessToken != null && data.RefreshToken != null)
            {
                _token.SaveTokens(
                    data.AccessToken,
                    data.RefreshToken,
                    data.ExpiresIn);
            }

            return data;
        }

        public async Task<string?> GetUserDetailsAsync()
        {
            var res = await _api.GetAsync("/api/auth/V1/Security-engine/Get/User-Details");
            if (res == null)
                return null;

            return await res.Content.ReadAsStringAsync();
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterUserDto registerUserDto)
        {
            var res = await _api.PostAsync(
                "/api/auth/V1/Security-engine/register",
                registerUserDto);

            var data = await res.Content.ReadFromJsonAsync<AuthResponseDto>();

            // Invalid API response
            if (data == null)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Error = "Invalid server response"
                };
            }

            // Transport error
            if (!res.IsSuccessStatusCode)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Error = data.Error ?? "Registration failed"
                };
            }

            // Business logic error
            if (!data.Success)
            {
                return data;
            }

            // Save tokens
            if (data.AccessToken != null && data.RefreshToken != null)
            {
                _token.SaveTokens(
                    data.AccessToken,
                    data.RefreshToken,
                    data.ExpiresIn);
            }

            return data;
        }

        public async Task LogoutAsync()
        {
            try
            {
                await _api.PostAsync("/api/auth/v1/security-engine/logout", new { });
            }
            catch
            {
                // Even if API call fails, clear local tokens
            }
        }

        /// <summary>
        /// Refresh the access token using the refresh token
        /// Called when access token expires
        /// </summary>
        public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
        {
            var res = await _api.PostAsync(
                "/api/auth/V1/Security-engine/refresh",
                new { RefreshToken = refreshToken });

            if (!res.IsSuccessStatusCode)
                return new AuthResponse { Success = false };

            var data = await res.Content.ReadFromJsonAsync<AuthResponse>();

            if (data == null || !data.Success)
                return new AuthResponse { Success = false };

            // ✅ Save new tokens
            _token.SaveTokens(
                data.AccessToken ?? "",
                data.RefreshToken ?? "",
                data.ExpiresIn);

            return data;
        }

    }
}

