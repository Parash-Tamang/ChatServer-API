namespace AIChatbot.web.Services
{
    public class TokenService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TokenService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Save access and refresh tokens to secure HTTP-only cookies
        /// </summary>
        public void SaveTokens(string accessToken, string refreshToken, int expiresIn)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return;

            // ? Save access token with expiration
            httpContext.Response.Cookies.Append("accessToken", accessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddSeconds(expiresIn)
            });

            // ? Save refresh token (typically 7 days)
            httpContext.Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });
        }

        /// <summary>
        /// Get access token from cookies
        /// </summary>
        public string? GetAccessToken()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.Request.Cookies.TryGetValue("accessToken", out var token) ?? false)
            {
                return token;
            }
            return null;
        }

        /// <summary>
        /// Get refresh token from cookies
        /// </summary>
        public string? GetRefreshToken()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.Request.Cookies.TryGetValue("refreshToken", out var token) ?? false)
            {
                return token;
            }
            return null;
        }

        /// <summary>
        /// Clear both tokens from cookies (on logout or token expiration)
        /// </summary>
        public void ClearTokens()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return;

            httpContext.Response.Cookies.Delete("accessToken");
            httpContext.Response.Cookies.Delete("refreshToken");
        }
    }
}
