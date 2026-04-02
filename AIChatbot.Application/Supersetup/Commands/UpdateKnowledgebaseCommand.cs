using MediatR;
using AIChatbot.Application.Common;

public record UpdateKnowledgebaseCommand(Guid ConnectionId)
    : IRequest<GenericResult>;