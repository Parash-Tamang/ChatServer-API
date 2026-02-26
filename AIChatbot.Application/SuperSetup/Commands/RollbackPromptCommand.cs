using MediatR;

namespace AIChatbot.Application.Supersetup.Commands;

public record RollbackPromptCommand(Guid DbId, string RequestedByUserId)
    : IRequest<bool>;