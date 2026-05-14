using AIChatbot.web.Interfaces;
using AIChatbot.web.Models.Settings;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace AIChatbot.web.Controllers
{
    public class SettingsController : Controller
    {
        private readonly ITokenService _tokenService;
        private readonly HttpClient _httpClient;

        public SettingsController(ITokenService tokenService, IHttpClientFactory httpClientFactory)
        {
            _tokenService = tokenService;
            _httpClient = httpClientFactory.CreateClient();
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> UserProfile()
        {
            var token = _tokenService.GetAccessToken();

            if (string.IsNullOrEmpty(token))
                return Unauthorized(new { message = "Session expired. Please log in again." });

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync(
                "http://192.168.40.101:5197/api/auth/V1/Security-engine/Get/User-Details");

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, new { message = "Failed to fetch profile." });

            var profile = await response.Content.ReadFromJsonAsync<UserProfileViewModel>();

            return Json(profile);
        }
    }
}