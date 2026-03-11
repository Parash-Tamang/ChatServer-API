using AIChatbot.web.Models.Auth;
using AIChatbot.web.Interfaces;
using AIChatbot.web.Services;
using Microsoft.AspNetCore.Mvc;

namespace AIChatbot.web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly TokenService _tokenService;

        public AuthController(IAuthService authService, TokenService tokenService)
        {
            _authService = authService;
            _tokenService = tokenService;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl)
        {
            // ✅ Check if user is already logged in
            var accessToken = _tokenService.GetAccessToken();
            
            if (!string.IsNullOrEmpty(accessToken))
            {
                // ✅ User already has a valid token
                // If returnUrl provided, go there. Otherwise go to Chat
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index", "Chat");
            }

            // Store returnUrl for use after login
            if (!string.IsNullOrEmpty(returnUrl))
            {
                ViewData["ReturnUrl"] = returnUrl;
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password, string? returnUrl)
        {
            var req = new LoginRequest { Email = email, Password = password };

            // Server-side validation
            var (isValid, errorMessage) = _authService.ValidateLoginInput(req);
            if (!isValid)
            {
                ViewData["ErrorMessage"] = errorMessage;
                if (!string.IsNullOrEmpty(returnUrl))
                    ViewData["ReturnUrl"] = returnUrl;
                return View();
            }

            var result = await _authService.LoginAsync(req);

            if (!result.Success)
            {
                ViewData["ErrorMessage"] = "Invalid email or password";
                if (!string.IsNullOrEmpty(returnUrl))
                    ViewData["ReturnUrl"] = returnUrl;
                return View();
            }

            // ✅ If returnUrl provided, redirect there. Otherwise go to Chat
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Chat");
        }

        [HttpGet]
        public IActionResult Register()
        {
            // ✅ Check if user is already logged in
            var accessToken = _tokenService.GetAccessToken();
            
            if (!string.IsNullOrEmpty(accessToken))
            {
                // ✅ Already logged in, redirect to Chat
                return RedirectToAction("Index", "Chat");
            }

            return View();
        }



        [HttpPost]
        public async Task<IActionResult> Register(RegisterRequest registerRequestDto)

        {
            if (!ModelState.IsValid)
                return View(registerRequestDto); 

            var req = new RegisterRequest
            {
                FirstName = registerRequestDto.FirstName,
                LastName = registerRequestDto.LastName,
                Phone = registerRequestDto.Phone,
                Email = registerRequestDto.Email,
                Password = registerRequestDto.Password,
                Role = registerRequestDto.Role
            };

            var result = await _authService.RegisterAsync(req);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Error ?? "Registration failed");
                ViewData["ErrorMessage"] = "Registration failed";
                return View(registerRequestDto);
            }

            // ✅ After registration, redirect to Chat
            return RedirectToAction("Index", "Chat");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            // ✅ Call API logout to revoke tokens on server
            await _authService.LogoutAsync();
            
            // ✅ Clear tokens from cookies
            _tokenService.ClearTokens();
            
            return RedirectToAction("Login");
        }

        [HttpGet]
        public async Task<IActionResult> UserDetails()
        {
            var json = await _authService.GetUserDetailsAsync();
            return Content(json, "application/json");
        }
    }
}
