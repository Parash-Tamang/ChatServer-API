using AIChatbot.web.Dto;
using AIChatbot.web.Interfaces;
using AIChatbot.web.Models.Auth;
using AIChatbot.web.Services;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace AIChatbot.web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly TokenService _tokenService;
        private readonly IRoleManagerService _roleManagerService;
        private readonly IValidator<RegisterUser> _validator;

        public AuthController(
            IValidator<RegisterUser> validator,TokenService tokenService,
            IRoleManagerService roleManagerService,
            IAuthService authService)
        {
            _validator = validator;
            _tokenService = tokenService;
            _roleManagerService = roleManagerService;
            _authService = authService;
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
            
                return View();
            }



        [HttpGet]
        public async Task<IActionResult> Register()
        {
            var accessToken = _tokenService.GetAccessToken();
            
            if (!string.IsNullOrEmpty(accessToken))
            {
                return RedirectToAction("Index", "Chat");
            }

            var model = new RegisterUser();
            model.Roles = await _roleManagerService.ListRoles();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterUser registerUser)
        {
            try
            {
                // Roles needed for dropdown and validator
                registerUser.Roles = await _roleManagerService.ListRoles();
            }
            catch
            {
                TempData["Error"] = "Unable to load roles. Please try again later.";
                return View(registerUser);
            }

            // Run FluentValidation
            var validationResult = await _validator.ValidateAsync(registerUser);

            if (!validationResult.IsValid)
            {
                TempData["ValidationErrors"] = validationResult.Errors
                    .Select(e => e.ErrorMessage)
                    .Distinct()
                    .ToList();

                return View(registerUser);
            }

            var dto = new RegisterUserDto
            {
                FirstName = registerUser.FirstName,
                LastName = registerUser.LastName,
                Email = registerUser.Email,
                Phone = registerUser.Phone,
                Password = registerUser.Password,
                Role = registerUser.SelectedRole
            };

            var result = await _authService.RegisterAsync(dto);

            if (!result.Success)
            {
                TempData["Error"] = result.Error ?? "Registration failed";
                return View(registerUser);
            }

            TempData["Success"] = "Registration successful";

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

      
    }
}
