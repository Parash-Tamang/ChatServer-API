using AIChatbot.web.Models.Auth;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace AIChatbot.web.Services
{
    public class TokenService
    {
        private readonly IHttpClientFactory _factory;
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _context;

        public TokenService(
            IHttpClientFactory factory,
            IConfiguration config,
            IHttpContextAccessor context)
        {
            _factory = factory;
            _config = config;
            _context = context;
        }

        public string? GetAccessToken()
        {
            return _context.HttpContext?.Request.Cookies["accessToken"];
        }

        public string? GetRefreshToken()
        {
            return _context.HttpContext?.Request.Cookies["refreshToken"];
        }

        public void SaveTokens(string access, string refresh, int expiresIn)
        {
            var http = _context.HttpContext;
            if (http == null) return;

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddSeconds(expiresIn)
            };

            http.Response.Cookies.Append("accessToken", access, cookieOptions);
            http.Response.Cookies.Append("refreshToken", refresh, cookieOptions);
        }

        public Task<string?> GetValidAccessToken()
        {
            var accessToken = GetAccessToken();

            if (string.IsNullOrEmpty(accessToken))
                return Task.FromResult<string?>(null);

            return Task.FromResult<string?>(accessToken);
        }

    }
}
