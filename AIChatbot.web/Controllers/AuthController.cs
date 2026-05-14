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
       
        private readonly IValidator<RegisterUser> _registerValidator;
        private readonly IValidator<LoginUser> _loginValidator;
        private readonly IPasswordResetService _passwordResetService;
        private readonly IRegistrationService _registrationService;
        private readonly ApiClient _api;

        public AuthController(
            IValidator<RegisterUser> registerValidator,
            ITokenService tokenService,
           
            IAuthService authService,
            IValidator<LoginUser> loginValidator,
            IPasswordResetService passwordResetService,
            IRegistrationService registrationService,
            ApiClient api)
        {
            _registerValidator = registerValidator;
            _loginValidator = loginValidator;
            _tokenService = tokenService;
           
            _authService = authService;
            _passwordResetService = passwordResetService;
            _registrationService = registrationService;
            _api = api;
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
        [AutoValidateAntiforgeryToken]
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
                return RedirectToAction("Index", "Chat");

            return View(new RegisterUser());
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterUser registerUser)
        {
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
                Token = registerUser.Token,
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
        // ── FORGOT PASSWORD ──────────────────────────────────────
        [HttpGet]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordModel model)
        {
            var result = await _passwordResetService.SendOtpAsync(new ForgotPasswordDto
            {
                Email = model.Email
            });

            if (!result.Success)
            {
                TempData["Error"] = result.Error;
                return RedirectToAction("ForgotPassword");
            }

            TempData["Success"] = "OTP sent to your email.";
            return RedirectToAction("VerifyOtp", new { email = model.Email });
        }

        // ── VERIFY OTP ───────────────────────────────────────────
        [HttpGet]
        public IActionResult VerifyOtp(string email)
            => View(new VerifyOtpModel { Email = email });

        [HttpPost]
        public async Task<IActionResult> VerifyOtp(VerifyOtpModel model)
        {
            var result = await _passwordResetService.VerifyOtpAsync(new VerifyOtpDto
            {
                Email = model.Email,
                Otp = model.Otp
            });

            if (!result.Success)
            {
                TempData["Error"] = result.Error;
                return RedirectToAction("VerifyOtp", new { email = model.Email });
            }

            // API sends reset link to email, just tell user to check email
            TempData["Success"] = "OTP verified. Please check your email for the reset link.";
            return RedirectToAction("Login");
        }

        // ── RESET PASSWORD ───────────────────────────────────────
        // User lands here by clicking the link in their email
        [HttpGet]
        public IActionResult ResetPassword(string token)
            => View(new ResetPasswordModel { Token = token });

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            if (model.NewPassword != model.ConfirmPassword)
            {
                TempData["Error"] = "Passwords do not match.";
                return RedirectToAction("ResetPassword", new { token = model.Token });
            }

            var result = await _passwordResetService.ResetPasswordAsync(new ResetPasswordDto
            {
                ResetToken = model.Token,
                NewPassword = model.NewPassword
            });

            if (!result.Success)
            {
                TempData["Error"] = result.Error;
                return RedirectToAction("ResetPassword", new { token = model.Token });
            }

            TempData["Success"] = "Password reset successful. Please login.";
            return RedirectToAction("Login");
        }
        // ── SEND REGISTER OTP ─────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> SendRegisterOtp([FromBody] SendRegisterOtpDto dto)
        {
            var result = await _registrationService.SendRegisterOtpAsync(dto);
            return Json(new { success = result.Success, error = result.Error });
        }

     
        // ── VERIFY REGISTER OTP ───────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> VerifyRegisterOtp([FromBody] VerifyRegisterOtpDto dto)
        {
            var result = await _registrationService.VerifyRegisterOtpAsync(dto);
            return Json(new { success = result.Success, error = result.Error, registerToken = result.RegisterToken });
            //                                                                
        }
        // ── to  get roles by using token
        [HttpGet]
        public async Task<IActionResult> GetRoles([FromQuery] string token)
        {
            var res = await _api.GetAsync(
                $"/api/auth/V1/Security-engine/roles?token={token}");

            if (res == null || !res.IsSuccessStatusCode)
                return Json(new List<string>());

            var data = await res.Content.ReadFromJsonAsync<RoleListResponseDto>();
            return Json(data?.Roles ?? new List<string>());
        }
    }
}
