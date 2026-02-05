using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Auth.Commands;
using AIChatbot.Application.Auth.Results;
using MediatR;

namespace AIChatbot.Application.Auth.Handlers;

public class RegisterUserHandler
    : IRequestHandler<RegisterUserCommand, AuthResult>
{
    private readonly IAuthService _auth;

    public RegisterUserHandler(IAuthService auth)
    {
        _auth = auth;
    }

    public async Task<AuthResult> Handle(
        RegisterUserCommand request,
        CancellationToken cancellationToken)
    {
        return await _auth.RegisterAsync(
            request.FirstName,
            request.LastName,
            request.Email,
            request.Phone,
            request.Password);
    }
}
