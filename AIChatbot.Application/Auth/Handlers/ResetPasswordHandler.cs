using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Auth.Commands;
using MediatR;

public class ResetPasswordHandler
    : IRequestHandler<ResetPasswordCommand, Unit>
{
    private readonly IAuthService _auth;

    public ResetPasswordHandler(IAuthService auth)
    {
        _auth = auth;
    }

    public async Task<Unit> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        await _auth.ResetPasswordAsync(
            request.Email,
            request.Token,
            request.NewPassword);

        return Unit.Value;
    }
}
