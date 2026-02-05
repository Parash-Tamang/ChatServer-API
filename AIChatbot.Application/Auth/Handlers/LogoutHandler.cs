using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Auth.Commands;
using MediatR;

public class LogoutHandler : IRequestHandler<LogoutCommand, Unit>
{
    private readonly IAuthService _auth;

    public LogoutHandler(IAuthService auth)
    {
        _auth = auth;
    }

    public async Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        await _auth.RevokeAllAsync(request.UserId);
        return Unit.Value;
    }
}
