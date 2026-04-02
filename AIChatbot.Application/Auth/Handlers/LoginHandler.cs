using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Auth.Commands;
using AIChatbot.Application.Auth.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
namespace AIChatbot.Application.Auth.Handlers;

public class LoginHandler
    : IRequestHandler<LoginCommand, AuthResult>
{
    private readonly IAuthService _auth;

    public LoginHandler(IAuthService auth)
    {
        _auth = auth;
    }

    public async Task<AuthResult> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            throw new BadHttpRequestException("Email is required.");

        if (string.IsNullOrWhiteSpace(request.Password))
            throw new BadHttpRequestException("Password is required.");

        var result = await _auth.LoginAsync(
            request.Email,
            request.Password);

        if (!result.Success)
            throw new UnauthorizedAccessException("Invalid email or password.");

        return result;
    }
}