using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using AIChatbot.web.Services;
using AIChatbot.web.Interfaces;

namespace AIChatbot.web.Filters
{
    /// <summary>
    /// Authorization filter that validates JWT tokens from cookies
    /// and redirects to login if token is missing or invalid
    /// </summary>
    public class AuthFilter : IActionFilter
    {
        private readonly ITokenService _token;
        private readonly IAuthService _authService;

        public AuthFilter(ITokenService token, IAuthService authService)
        {
            _token = token;
            _authService = authService;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var http = context.HttpContext;
            var accessToken = http.Request.Cookies["accessToken"];

            // ✅ No token? Redirect to login with return URL
            if (string.IsNullOrEmpty(accessToken))
            {
                var returnUrl = http.Request.Path + http.Request.QueryString;
                context.Result = new RedirectToActionResult("Login", "Auth", new { returnUrl });
                return;
            }

            // ✅ Check if token is expired
            if (IsTokenExpired(accessToken))
            {
                // Try to refresh token
                var refreshToken = http.Request.Cookies["refreshToken"];

                if (!string.IsNullOrEmpty(refreshToken))
                {
                    // Note: In a real application, you might want to refresh synchronously here
                    // or store a flag to refresh on next request
                    _token.ClearTokens(); // Clear invalid tokens
                    var returnUrl = http.Request.Path + http.Request.QueryString;
                    context.Result = new RedirectToActionResult("Login", "Auth", new { returnUrl });
                }
                else
                {
                    var returnUrl = http.Request.Path + http.Request.QueryString;
                    context.Result = new RedirectToActionResult("Login", "Auth", new { returnUrl });
                }
                return;
            }
        }

        public void OnActionExecuted(ActionExecutedContext context) { }

        /// <summary>
        /// Checks if JWT token is expired by parsing expiration claim
        /// Without external JWT library dependencies
        /// </summary>
        private static bool IsTokenExpired(string token)
        {
            try
            {
                // JWT format: header.payload.signature
                var parts = token.Split('.');
                if (parts.Length != 3)
                    return true;

                // Decode payload (it's base64url encoded)
                var payload = parts[1];

                // Add padding if needed
                var paddedPayload = payload.Length % 4 == 0 ? payload : payload + new string('=', 4 - payload.Length % 4);

                // Convert base64url to base64
                var base64 = paddedPayload.Replace('-', '+').Replace('_', '/');

                var jsonBytes = Convert.FromBase64String(base64);
                var json = System.Text.Encoding.UTF8.GetString(jsonBytes);

                // Parse JSON to get 'exp' claim
                if (json.Contains("\"exp\":"))
                {
                    var expStart = json.IndexOf("\"exp\":") + 6;
                    var expEnd = json.IndexOf(',', expStart);
                    if (expEnd == -1)
                        expEnd = json.IndexOf('}', expStart);

                    var expStr = json.Substring(expStart, expEnd - expStart).Trim();

                    if (long.TryParse(expStr, out var expirationUnixTime))
                    {
                        var expirationDate = UnixTimeStampToDateTime(expirationUnixTime);
                        return expirationDate < DateTime.UtcNow;
                    }
                }

                return false;
            }
            catch
            {
                // If we can't read the token, consider it invalid
                return true;
            }
        }

        private static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToUniversalTime();
            return dateTime;
        }

        private static string? ExtractRoleFromToken(string token)
        {
            try
            {
                var parts = token.Split('.');
                if (parts.Length != 3) return null;

                var payload = parts[1];
                var paddedPayload = payload.Length % 4 == 0 ? payload : payload + new string('=', 4 - payload.Length % 4);
                var base64 = paddedPayload.Replace('-', '+').Replace('_', '/');

                var jsonBytes = Convert.FromBase64String(base64);
                var json = System.Text.Encoding.UTF8.GetString(jsonBytes);

                if (json.Contains("\"role\":"))
                {
                    var roleStart = json.IndexOf("\"role\":\"") + 8;
                    var roleEnd = json.IndexOf('"', roleStart);
                    return json.Substring(roleStart, roleEnd - roleStart);
                }

                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}
