using MediatR;
using AIChatbot.Application.Auth.Results;

namespace AIChatbot.Application.Auth.Commands;

public record LoginCommand(
    string Email,
    string Password
) : IRequest<AuthResult>;
