using AIChatbot.web.Models.Auth;
using AIChatbot.web.Services;
using Microsoft.AspNetCore.Mvc;

namespace AIChatbot.web.Controllers
{
    public class AuthController : Controller
    {
        private readonly AuthApiService _auth;
        private readonly ApiClient _api;   // 👈 ADD THIS

        public AuthController(AuthApiService auth, ApiClient api) // 👈 INJECT HERE
        {
            _auth = auth;
            _api = api;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            var result = await _auth.LoginAsync(req);

            if (!result.Success)
                return Unauthorized(new { message = "Invalid login" });

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req)
        {
            var result = await _auth.RegisterAsync(req);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _api.PostAsync("/api/auth/v1/security-engine/logout", new { });
            Response.Cookies.Delete("accessToken");
            Response.Cookies.Delete("refreshToken");
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> UserDetails()
        {
            var res = await _api.GetAsync("/api/auth/V1/Security-engine/Get/User-Details");

            var json = await res.Content.ReadAsStringAsync();

            return Content(json, "application/json");
        }
  

    }
}
