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
        private readonly ITokenService _tokenService;
        private readonly IRoleManagerService _roleManagerService;
        private readonly IValidator<RegisterUser> _registerValidator;
        private readonly IValidator<LoginUser> _loginValidator;

        public AuthController(
            IValidator<RegisterUser> registerValidator,
            ITokenService tokenService,
            IRoleManagerService roleManagerService,
            IAuthService authService,
            IValidator<LoginUser> loginValidator)
        {
            _registerValidator = registerValidator;
            _loginValidator = loginValidator;
            _tokenService = tokenService;
            _roleManagerService = roleManagerService;
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            // ✅ Check if user is already logged in
            var accessToken = _tokenService.GetAccessToken();
            
            if (!string.IsNullOrEmpty(accessToken))
            {
                return RedirectToAction("Index", "Chat");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginUser loginUser)
        {
            // Run FluentValidation
            var validationResult = await _loginValidator.ValidateAsync(loginUser);

            if (!validationResult.IsValid)
            {
                TempData["ValidationErrors"] = validationResult.Errors
                    .Select(e => e.ErrorMessage)
                    .Distinct()
                    .ToList();

                return RedirectToAction("Login");
            }

            var dto = new LoginUserDto
            {
                Email = loginUser.Email,
                Password = loginUser.Password
            };

            var result = await _authService.LoginAsync(dto);

            if (!result.Success)
            {
                TempData["Error"] = result.Error ?? "Login failed";
                return RedirectToAction("Login");
            }

            TempData["Success"] = "Login successful";

            return RedirectToAction("Index", "Chat");
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
            RoleListDto roleList = await _roleManagerService.ListRoles();
            model.Roles = roleList.roles;

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterUser registerUser)
        {
            try
            {
                // Roles needed for dropdown and validator
                RoleListDto roleListDto = await _roleManagerService.ListRoles();
                registerUser.Roles = roleListDto.roles;
            }
            catch
            {
                TempData["Error"] = "Unable to load roles. Please try again later.";
                return RedirectToAction("Register");
            }

            // Run FluentValidation
            var validationResult = await _registerValidator.ValidateAsync(registerUser);

            if (!validationResult.IsValid)
            {
                TempData["ValidationErrors"] = validationResult.Errors
                    .Select(e => e.ErrorMessage)
                    .Distinct()
                    .ToList();

                return RedirectToAction("Register");
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
                return RedirectToAction("Register");
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
