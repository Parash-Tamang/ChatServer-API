using System.Security.Claims;

using AIChatbot.Api.Models.Auth;
using AIChatbot.Application.Auth.Commands;
using AIChatbot.Application.Auth.Queries;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AIChatbot.Api.Controllers;

[ApiController]

[Route("api/auth/V1/Security-engine")]

// ============================================================
// CONTROLLER
// ============================================================

public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    // ============================================================
    // GET ROLES
    // ============================================================

    [AllowAnonymous]

    [EnableRateLimiting("register")]

    [HttpGet("roles")]
    public async Task<IActionResult>
        GetRoles(
            [FromQuery]
            string token)
    {
        var roles =
            await _mediator.Send(
                new GetRolesQuery(token));

        return Ok(new
        {
            success = true,

            roles
        });
    }

    // ============================================================
    // REGISTER
    // ============================================================

    [AllowAnonymous]

    [EnableRateLimiting("register")]

    [HttpPost("register")]
    public async Task<IActionResult>
        Register(
            [FromBody]
            RegisterRequest r)
    {
        var result =
            await _mediator.Send(
                new RegisterUserCommand(
                    r.FirstName,

                    r.LastName,

                    r.Email,

                    r.Phone,

                    r.Password,

                    r.Role,

                    r.Token));

        return Created(
            string.Empty,
            result);
    }

    // ============================================================
    // SEND REGISTER OTP
    // ============================================================

    [AllowAnonymous]

    [EnableRateLimiting("register")]

    [HttpPost("send-register-otp")]
    public async Task<IActionResult>
        SendRegisterOtp(
            [FromBody]
            SendRegisterOtpCommand cmd)
    {
        await _mediator.Send(cmd);

        return Ok(new
        {
            success = true,

            message =
                "OTP sent successfully"
        });
    }

    // ============================================================
    // RESEND REGISTER OTP
    // ============================================================

    [AllowAnonymous]

    [EnableRateLimiting("register")]

    [HttpPost("resend-register-otp")]
    public async Task<IActionResult>
        ResendOtp(
            [FromBody]
            ResendRegisterOtpCommand cmd)
    {
        await _mediator.Send(cmd);

        return Ok(new
        {
            success = true,

            message =
                "OTP resent successfully"
        });
    }

    // ============================================================
    // VERIFY REGISTER OTP
    // ============================================================

    [AllowAnonymous]

    [EnableRateLimiting("register")]

    [HttpPost("verify-register-otp")]
    public async Task<IActionResult>
        VerifyRegisterOtp(
            [FromBody]
            VerifyRegisterOtpCommand cmd)
    {
        var token =
            await _mediator.Send(cmd);

        return Ok(new
        {
            success = true,

            registerToken = token
        });
    }

    // ============================================================
    // LOGIN
    // ============================================================

    [AllowAnonymous]

    [EnableRateLimiting("login")]

    [HttpPost("login")]
    public async Task<IActionResult>
        Login(
            [FromBody]
            LoginRequest r)
    {
        var result =
            await _mediator.Send(
                new LoginCommand(
                    r.Email,
                    r.Password));

        return Ok(result);
    }

    // ============================================================
    // REFRESH TOKEN
    // ============================================================

    [AllowAnonymous]

    [EnableRateLimiting("login")]

    [HttpPost("refresh")]
    public async Task<IActionResult>
        Refresh(
            [FromBody]
            RefreshTokenRequest r)
    {
        var result =
            await _mediator.Send(
                new RefreshTokenCommand(
                    r.RefreshToken));

        return Ok(result);
    }

    // ============================================================
    // LOGOUT
    // ============================================================

    [Authorize(Policy = "AuthenticatedUser")]

    [HttpPost("logout")]
    public async Task<IActionResult>
        Logout()
    {
        var userId =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier)
            ??
            User.FindFirstValue("sub")
            ??
            User.FindFirstValue("userId");

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new UnauthorizedAccessException(
                "Invalid token");
        }

        await _mediator.Send(
            new LogoutCommand(userId));

        return NoContent();
    }

    // ============================================================
    // USER PROFILE
    // ============================================================

    [Authorize(Policy = "AuthenticatedUser")]

    [HttpGet("Get/User-Details")]
    public async Task<IActionResult>
        Me()
    {
        var userId =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier)
            ??
            throw new UnauthorizedAccessException(
                "Invalid token");

        var profile =
            await _mediator.Send(
                new GetUserProfileQuery(
                    userId));

        return Ok(profile);
    }

    // ============================================================
    // FORGOT PASSWORD
    // ============================================================

    [AllowAnonymous]

    [EnableRateLimiting("login")]

    [HttpPost("forgot-password")]
    public async Task<IActionResult>
        ForgotPassword(
            [FromBody]
            ForgotPasswordCommand command)
    {
        var result =
            await _mediator.Send(command);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    // ============================================================
    // VERIFY RESET OTP
    // ============================================================

    [AllowAnonymous]

    [EnableRateLimiting("login")]

    [HttpPost("verify-otp")]
    public async Task<IActionResult>
        VerifyOtp(
            [FromBody]
            VerifyOtpCommand cmd)
    {
        await _mediator.Send(cmd);

        return Ok(new
        {
            success = true,

            message =
                "Reset link sent to email"
        });
    }

    // ============================================================
    // RESET PASSWORD
    // ============================================================

    [AllowAnonymous]

    [EnableRateLimiting("login")]

    [HttpPost("reset-password")]
    public async Task<IActionResult>
        ResetPassword(
            [FromBody]
            ResetPasswordCommand cmd)
    {
        await _mediator.Send(cmd);

        return Ok(new
        {
            success = true
        });
    }

    // ============================================================
    // CHANGE PASSWORD
    // ============================================================

    [Authorize(Policy = "AuthenticatedUser")]

    [HttpPost("change-password")]
    public async Task<IActionResult>
        ChangePassword(
            [FromBody]
            ChangePasswordCommand command)
    {
        var result =
            await _mediator.Send(command);

        return Ok(new
        {
            success = true,

            message =
                "Password changed successfully"
        });
    }
}