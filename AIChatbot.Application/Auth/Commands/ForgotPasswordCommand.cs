using MediatR;

namespace AIChatbot.Application.Auth.Commands;

public record ForgotPasswordCommand(string Email) : IRequest<Unit>;
