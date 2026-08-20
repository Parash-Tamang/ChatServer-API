using MediatR;

namespace AIChatbot.Application.Auth.Commands;

using AIChatbot.Application.Auth.Results;

public record ForgotPasswordCommand(string Email)
    : IRequest<ForgotPasswordResult>;
