using AIChatbot.web.Models.Auth;

namespace AIChatbot.web.Services
{
    public class AuthApiService
    {
        private readonly ApiClient _api;
        private readonly TokenService _token;

        public AuthApiService(ApiClient api, TokenService token)
        {
            _api = api;
            _token = token;
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest req)
        {
            var res = await _api.PostAsync(
                "/api/auth/V1/Security-engine/login",
                req);

            if (!res.IsSuccessStatusCode)
                return new AuthResponse { Success = false };

            var data = await res.Content.ReadFromJsonAsync<AuthResponse>();

            if (data == null || !data.Success)
                return new AuthResponse { Success = false };

            _token.SaveTokens(
    data.AccessToken ?? "",
    data.RefreshToken ?? "",
    3600
);


            return data;
        }
        public async Task<string> GetUserDetailsAsync()
        {
            var res = await _api.GetAsync("/api/auth/V1/Security-engine/Get/User-Details");
            return await res.Content.ReadAsStringAsync();
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest req)
        {
            var res = await _api.PostAsync(
                "/api/auth/V1/Security-engine/register",
                req);

            if (!res.IsSuccessStatusCode)
                return new AuthResponse { Success = false };

            return await res.Content.ReadFromJsonAsync<AuthResponse>()
                   ?? new AuthResponse { Success = false };
        }
    }
}
