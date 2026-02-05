using MediatR;

namespace AIChatbot.Application.Auth.Commands;

public record LogoutCommand(string UserId) : IRequest<Unit>;
