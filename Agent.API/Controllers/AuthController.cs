using Agent.API.Helpers;
using Agent.Application.AuthMediator.Command;
using Agent.Application.Dto.UserManagement;
using Agent.Application.Helpers;
using Agent.Domain.Entities.UserManagement;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
namespace Agent.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IValidator <UserRegisterDto>_registerValidate;
        private readonly IValidator<UserLoginDto> _loginValidate;
        private readonly IValidator<UserResetPasswordDto> _resetPasswordValidate;
        private readonly IValidator<UserForgotPasswordDto> _forgotPasswordValidate;
        private readonly IValidator<RefreshTokenRequestDto> _refreshTokenRequestValidate;

        public AuthController(IMediator mediator, IValidator<UserRegisterDto> registerValidate, IValidator<UserLoginDto> loginValidate, 
            IValidator<UserResetPasswordDto> resetPasswordValidate, IValidator<UserForgotPasswordDto> forgotPasswordValidate, IValidator<RefreshTokenRequestDto> refreshTokenRequestValidate)
        {
            _mediator = mediator;
            _registerValidate = registerValidate;
            _loginValidate = loginValidate;
            _forgotPasswordValidate = forgotPasswordValidate;
            _resetPasswordValidate = resetPasswordValidate;
            _refreshTokenRequestValidate = refreshTokenRequestValidate;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterDto userRegisterDto)
        { 
                var validationResult = await _registerValidate.ValidateAsync(userRegisterDto);
                if (!validationResult.IsValid)
                {
                    var problemDetails = ValidationProblemDetailsHelper.Build(validationResult, "Register Validation Failed");
                    return BadRequest(problemDetails);
                }
                ApiResult<AuthTokenResponseDto> response = await _mediator.Send(new RegisterCommand(userRegisterDto));
            
            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto userLoginDto)
        {
            var validationResult = await _loginValidate.ValidateAsync(userLoginDto);
            if (!validationResult.IsValid)
            {
                var problemDetails = ValidationProblemDetailsHelper.Build(validationResult, "Login Validation Failed");
                return BadRequest(problemDetails);
            }
            ApiResult<AuthTokenResponseDto> response = await _mediator.Send(new LoginCommand(userLoginDto));

            if (!response.Success || response.Data == null)
                return BadRequest(response);

            var token = response.Data.RefreshToken;

            Response.Cookies.Append(
            "refreshToken",
            token,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.Now.AddDays(7)
            });

            return Ok(ApiResult<AuthTokenResponseDto>.Ok(new AuthTokenResponseDto
            {
                User = response.Data.User,
                AccessToken = response.Data.AccessToken,
                ExpiresIn = response.Data.ExpiresIn
            }, response.Message));
        }

        [AllowAnonymous]
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] UserForgotPasswordDto userForgotPasswordDto)
        {
            var validationResult = await _forgotPasswordValidate.ValidateAsync(userForgotPasswordDto);
            if (!validationResult.IsValid)
            {
                var problemDetails = ValidationProblemDetailsHelper.Build(validationResult, "Email Verification Failed");
                return BadRequest(problemDetails);
            }
            ApiResult<object> response = await _mediator.Send(new ForgotPasswordCommand(userForgotPasswordDto));

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }

        [AllowAnonymous]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] UserResetPasswordDto userResetPasswordDto)
        {
            var validationResult = await _resetPasswordValidate.ValidateAsync(userResetPasswordDto);
            if (!validationResult.IsValid)
            {
                var problemDetails = ValidationProblemDetailsHelper.Build(validationResult, "Validation Failed");
                return BadRequest(problemDetails);
            }
            ApiResult<object> response = await _mediator.Send(new ResetPasswordCommand(userResetPasswordDto));

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken()
        {

            if (!Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
            {
                return Unauthorized(
                    ApiResult<object>.Fail("Refresh token missing")
                );
            }

            var token = new RefreshTokenRequestDto { RefreshToken = refreshToken };

            var validationResult = await _refreshTokenRequestValidate.ValidateAsync(token);
            if (!validationResult.IsValid)
            {
                var problemDetails = ValidationProblemDetailsHelper.Build(validationResult, "Token Generation Failed");
                return BadRequest(problemDetails);
            }

            ApiResult<AuthTokenResponseDto> response = await _mediator.Send(new RefreshTokenCommand(token));

            if (!response.Success || response.Data == null)
            {
                return Unauthorized(response);
            }

            //Response.Cookies.Append(
            //"refreshToken",
            //response.Data.RefreshToken,
            //new CookieOptions
            //{
            //    HttpOnly = true,
            //    Secure = true,
            //    SameSite = SameSiteMode.None,
            //    Expires = DateTime.Now.AddDays(7)
            //});

            return Ok(ApiResult<AuthTokenResponseDto>.Ok(new AuthTokenResponseDto
            {
                User = response.Data.User,
                AccessToken = response.Data.AccessToken,
                RefreshToken = "Send in Cookies",
                ExpiresIn = response.Data.ExpiresIn
            },response.Message));
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            if (Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
            {
              
                await _mediator.Send(new LogoutCommand(refreshToken));
            }
            // Always remove cookie
            Response.Cookies.Delete("refreshToken", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/"
            });

            return Ok(ApiResult<object>.Ok(null, "Logged out"));
        }
    }
}
