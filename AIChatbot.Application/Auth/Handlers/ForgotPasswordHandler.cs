using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Auth.Commands;
using MediatR;

namespace AIChatbot.Application.Auth.Handlers;

public class ForgotPasswordHandler
    : IRequestHandler<ForgotPasswordCommand, Unit>
{
    private readonly IAuthService _auth;

    public ForgotPasswordHandler(IAuthService auth)
    {
        _auth = auth;
    }

    public async Task<Unit> Handle(
        ForgotPasswordCommand request,
        CancellationToken cancellationToken)
    {
        await _auth.ForgotPasswordAsync(request.Email);
        return Unit.Value;
    }
}
