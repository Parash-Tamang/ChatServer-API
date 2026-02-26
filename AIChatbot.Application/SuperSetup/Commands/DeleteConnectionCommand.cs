using MediatR;

namespace AIChatbot.Application.Supersetup.Commands;

public record DeleteConnectionCommand(Guid DbId, string RequestedByUserId)
    : IRequest<bool>;