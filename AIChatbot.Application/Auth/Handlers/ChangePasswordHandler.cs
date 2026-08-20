using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Auth.Commands;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

public class ChangePasswordHandler
    : IRequestHandler<ChangePasswordCommand, bool>
{
    private readonly IAuthService _auth;
    private readonly IHttpContextAccessor _http;

    public ChangePasswordHandler(
        IAuthService auth,
        IHttpContextAccessor http)
    {
        _auth = auth;
        _http = http;
    }

    public async Task<bool> Handle(ChangePasswordCommand request, CancellationToken ct)
    {
        // 🔐 get user from JWT
        var userId = _http.HttpContext?
            .User
            .FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("Invalid user");

        // 🔐 change password
        var result = await _auth.ChangePasswordAsync(
            userId,
            request.CurrentPassword,
            request.NewPassword);

        if (!result)
            throw new UnauthorizedAccessException("Current password is incorrect");

        // 🔥 ADD THIS HERE (VERY IMPORTANT)
        await _auth.RevokeAllAsync(userId);

        return true;
    }
}