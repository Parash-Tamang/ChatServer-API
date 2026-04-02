using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Auth.Commands;
using MediatR;
using Microsoft.AspNetCore.Http;

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
        if (string.IsNullOrWhiteSpace(request.Email))
            throw new BadHttpRequestException("Email is required.");

        await _auth.ForgotPasswordAsync(request.Email);

        return Unit.Value;
    }
}