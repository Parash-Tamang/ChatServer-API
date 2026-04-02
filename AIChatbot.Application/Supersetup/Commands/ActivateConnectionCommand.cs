using MediatR;
using AIChatbot.Application.Common;

public record ActivateConnectionCommand(Guid ConnectionId)
    : IRequest<GenericResult>;