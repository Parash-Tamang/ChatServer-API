using MediatR;

namespace AIChatbot.Application.Auth.Commands;

public record ResetPasswordCommand(
    string Email,
    string Token,
    string NewPassword
) : IRequest<Unit>;
