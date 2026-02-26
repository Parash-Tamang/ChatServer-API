using AIChatbot.Api.Models.Auth;
using AIChatbot.Application.Auth.Commands;
using AIChatbot.Application.Auth.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AIChatbot.Api.Controllers;

[ApiController]
[Route("api/auth/V1/Security-engine")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // ---------------- REGISTER ----------------
    // Creates a new user account
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest r)
    {
        var result = await _mediator.Send(
            new RegisterUserCommand(
                r.FirstName,
                r.LastName,
                r.Email,
                r.Phone,
                r.Password,
                r.Role));

        // 201 Created for new resource
        return Created(string.Empty, result);
    }

    // ---------------- LOGIN ----------------
    // Authenticates user and returns access & refresh tokens
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest r)
    {
        var result = await _mediator.Send(
            new LoginCommand(r.Email, r.Password));

        return Ok(result);
    }

    // ---------------- REFRESH TOKEN ----------------
    // Issues a new access token using refresh token
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest r)
    {
        var result = await _mediator.Send(
            new RefreshTokenCommand(r.RefreshToken));

        return Ok(result);
    }

    // ---------------- LOGOUT ----------------
    // Revokes all refresh tokens for current user
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var userId =
            User.FindFirstValue(ClaimTypes.NameIdentifier) ??
            User.FindFirstValue("sub") ??
            User.FindFirstValue("userId");

        if (string.IsNullOrWhiteSpace(userId))
            throw new UnauthorizedAccessException("Invalid token");

        await _mediator.Send(new LogoutCommand(userId));
        return NoContent();
    }

    // ---------------- USER PROFILE ----------------
    // Returns logged-in user details
    [Authorize]
    [HttpGet("Get/User-Details")]
    public async Task<IActionResult> Me()
    {
        var userId =
            User.FindFirstValue(ClaimTypes.NameIdentifier) ??
            throw new UnauthorizedAccessException("Invalid token");

        var profile = await _mediator.Send(
            new GetUserProfileQuery(userId));

        return Ok(profile);
    }

    // ---------------- FORGOT PASSWORD ----------------
    // Sends password reset token
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest r)
    {
        await _mediator.Send(new ForgotPasswordCommand(r.Email));
        return NoContent();
    }

    // ---------------- RESET PASSWORD ----------------
    // Resets password using reset token
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest r)
    {
        await _mediator.Send(
            new ResetPasswordCommand(r.Email, r.Token, r.NewPassword));

        return NoContent();
    }
}