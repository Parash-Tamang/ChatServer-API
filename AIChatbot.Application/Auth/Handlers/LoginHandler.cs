using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Auth.Commands;
using AIChatbot.Application.Auth.Results;
using MediatR;

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
        var result = await _auth.LoginAsync(
            request.Email,
            request.Password);

        if (!result.Success)
            throw new UnauthorizedAccessException(result.Error ?? "Invalid credentials");

        return result;
    }
}