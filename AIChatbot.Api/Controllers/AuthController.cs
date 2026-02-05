using AIChatbot.Api.Models.Auth;
using AIChatbot.Application.Auth.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AIChatbot.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

   
    // Registers a new user
   
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest r)
    {
        var result = await _mediator.Send(
            new RegisterUserCommand(
                r.FirstName,
                r.LastName,
                r.Email,
                r.Phone,
                r.Password));

        return Ok(result);
    }


    /// Authenticates user and returns access & refresh tokens
   
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest r)
    {
        var result = await _mediator.Send(
            new LoginCommand(r.Email, r.Password));

        return Ok(result);
    }

  
    /// Issues a new access token using refresh token
   
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest r)
    {
        var result = await _mediator.Send(
            new RefreshTokenCommand(r.RefreshToken));

        return Ok(result);
    }

  
    // Logs out the currently authenticated user
   
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        // SAFER: Try common claim types instead of assuming NameIdentifier exists
        var userId =
            User.FindFirstValue(ClaimTypes.NameIdentifier) ??
            User.FindFirstValue("sub") ??
            User.FindFirstValue("userId");

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized("Invalid token: user identifier missing.");

        await _mediator.Send(new LogoutCommand(userId));
        return NoContent();
    }

  
    //Sends password reset token to user's email

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest r)
    {
        await _mediator.Send(new ForgotPasswordCommand(r.Email));
        return NoContent();
    }

 
    // Resets user password using reset token
    
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest r)
    {
        await _mediator.Send(
            new ResetPasswordCommand(r.Email, r.Token, r.NewPassword));

        return NoContent();
    }
}
