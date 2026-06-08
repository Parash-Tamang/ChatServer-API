using AIChatbot.web.Interfaces;
using AIChatbot.web.Models.Settings;
using Microsoft.AspNetCore.Mvc;

namespace AIChatbot.web.Controllers
{
    public class SettingsController : Controller
    {
        private readonly ITokenService _tokenService;
        private readonly Services.AuthService _authService;

        public SettingsController(ITokenService tokenService, Services.AuthService authService)
        {
            _tokenService = tokenService;
            _authService = authService;
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

            var profileJson = await _authService.GetUserDetailsAsync();

            if (string.IsNullOrWhiteSpace(profileJson))
                return StatusCode(503, new { message = "Failed to fetch profile." });

            return Content(profileJson, "application/json");
        }
    }
}