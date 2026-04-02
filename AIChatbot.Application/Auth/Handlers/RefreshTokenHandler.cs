using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Auth.Commands;
using AIChatbot.Application.Auth.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
namespace AIChatbot.Application.Auth.Handlers;

public class RefreshTokenHandler
    : IRequestHandler<RefreshTokenCommand, AuthResult>
{
    private readonly IAuthService _auth;

    public RefreshTokenHandler(IAuthService auth)
    {
        _auth = auth;
    }

    public async Task<AuthResult> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            throw new BadHttpRequestException("Refresh token is required.");

        var result = await _auth.RefreshTokenAsync(request.RefreshToken);

        if (!result.Success)
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");

        return result;
    }
}