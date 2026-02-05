using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Auth.Commands;
using AIChatbot.Application.Auth.Results;
using MediatR;

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
        return await _auth.RefreshTokenAsync(request.RefreshToken);
    }
}
