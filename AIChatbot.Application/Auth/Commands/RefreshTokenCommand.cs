using MediatR;
using AIChatbot.Application.Auth.Results;

namespace AIChatbot.Application.Auth.Commands;

public record RefreshTokenCommand(string RefreshToken)
    : IRequest<AuthResult>;
