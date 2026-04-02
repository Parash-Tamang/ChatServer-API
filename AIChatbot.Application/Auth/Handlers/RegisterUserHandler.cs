using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Auth.Commands;
using AIChatbot.Application.Auth.Results;
using MediatR;
using Microsoft.AspNetCore.Http;

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
        if (string.IsNullOrWhiteSpace(request.Email))
            throw new BadHttpRequestException("Email is required.");

        if (string.IsNullOrWhiteSpace(request.Password))
            throw new BadHttpRequestException("Password is required.");

        var result = await _auth.RegisterAsync(
            request.FirstName,
            request.LastName,
            request.Email,
            request.Phone,
            request.Password,
            request.Role
        );

        if (!result.Success)
            throw new InvalidOperationException(result.Error ?? "User registration failed.");

        return result;
    }
}