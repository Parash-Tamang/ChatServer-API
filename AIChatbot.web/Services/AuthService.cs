using AIChatbot.web.Models.Auth;
using AIChatbot.web.Interfaces;
using System.Text.RegularExpressions;

namespace AIChatbot.web.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApiClient _api;
        private readonly TokenService _token;

        public AuthService(ApiClient api, TokenService token)
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

            // ✅ Save both tokens - AccessToken for API calls, RefreshToken for renewal
            _token.SaveTokens(
                data.AccessToken ?? "",
                data.RefreshToken ?? "",
                data.ExpiresIn);

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

            var data = await res.Content.ReadFromJsonAsync<AuthResponse>();

            if (data == null)
                return new AuthResponse { Success = false };

            // ✅ Save tokens after registration
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

        /// <summary>
        /// Validates login input
        /// </summary>
        public (bool isValid, string errorMessage) ValidateLoginInput(LoginRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Email))
                return (false, "Enter email");

            if (string.IsNullOrWhiteSpace(req.Password))
                return (false, "Enter password");

            // Validate email format
            var emailRegex = new Regex(@"^[^\s@]+@[^\s@]+\.[^\s@]+$");
            if (!emailRegex.IsMatch(req.Email))
                return (false, "Invalid email format");

            return (true, "");
        }

        /// <summary>
        /// Validates registration input with multiple field validation
        /// </summary>
        public (bool isValid, Dictionary<string, string> errors) ValidateRegisterInput(RegisterRequest req)
        {
            var errors = new Dictionary<string, string>();

            // First name
            if (string.IsNullOrWhiteSpace(req.FirstName))
                errors["firstName"] = "Enter first name";

            // Last name
            if (string.IsNullOrWhiteSpace(req.LastName))
                errors["lastName"] = "Enter last name";

            // Email
            var emailRegex = new Regex(@"^[^\s@]+@[^\s@]+\.[^\s@]+$");
            if (string.IsNullOrWhiteSpace(req.Email))
                errors["email"] = "Enter email";
            else if (!emailRegex.IsMatch(req.Email))
                errors["email"] = "Invalid email format";

            // Phone
            var phoneRegex = new Regex(@"^\d{10}$");
            if (string.IsNullOrWhiteSpace(req.Phone))
                errors["phone"] = "Enter phone number";
            else if (!phoneRegex.IsMatch(req.Phone.Replace("+91", "")))
                errors["phone"] = "Enter valid 10 digit number";

            // Password
            var passRegex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&]).{8,}$");
            if (string.IsNullOrWhiteSpace(req.Password))
                errors["password"] = "Enter password";
            else if (!passRegex.IsMatch(req.Password))
                errors["password"] = "8+ chars, upper, lower, number & special";

            return (errors.Count == 0, errors);
        }
    }
}
