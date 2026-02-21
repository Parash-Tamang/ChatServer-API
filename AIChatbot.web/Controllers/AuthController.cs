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
        public async Task<IActionResult> Register(string firstName, string lastName, string email, string phone, string password, string confirmPassword)
        {
            var req = new RegisterRequest
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Phone = phone,
                Password = password
            };

            // Server-side validation
            var (isValid, errors) = _authService.ValidateRegisterInput(req);
            if (!isValid)
            {
                // Set ViewData for each error field
                if (errors.ContainsKey("firstName"))
                    ViewData["FirstNameError"] = errors["firstName"];
                if (errors.ContainsKey("lastName"))
                    ViewData["LastNameError"] = errors["lastName"];
                if (errors.ContainsKey("email"))
                    ViewData["EmailError"] = errors["email"];
                if (errors.ContainsKey("phone"))
                    ViewData["PhoneError"] = errors["phone"];
                if (errors.ContainsKey("password"))
                    ViewData["PasswordError"] = errors["password"];

                return View();
            }

            // Validate password confirmation on server
            if (password != confirmPassword)
            {
                ViewData["ConfirmPasswordError"] = "Passwords do not match";
                return View();
            }

            // Add +91 prefix to phone if not already present
            if (!phone.StartsWith("+91"))
                phone = "+91" + phone;

            req.Phone = phone;

            var result = await _authService.RegisterAsync(req);

            if (!result.Success)
            {
                ViewData["ErrorMessage"] = "Registration failed";
                return View();
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
