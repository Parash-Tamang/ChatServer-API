using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Auth.Commands;
using MediatR;
using Microsoft.AspNetCore.Http;
public class ResetPasswordHandler
    : IRequestHandler<ResetPasswordCommand, Unit>
{
    private readonly IAuthService _auth;

    public ResetPasswordHandler(IAuthService auth)
    {
        _auth = auth;
    }

    public async Task<Unit> Handle(
        ResetPasswordCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            throw new BadHttpRequestException("Email is required.");

        if (string.IsNullOrWhiteSpace(request.Token))
            throw new BadHttpRequestException("Reset token is required.");

        if (string.IsNullOrWhiteSpace(request.NewPassword))
            throw new BadHttpRequestException("New password is required.");

        await _auth.ResetPasswordAsync(
            request.Email,
            request.Token,
            request.NewPassword);

        return Unit.Value;
    }
}