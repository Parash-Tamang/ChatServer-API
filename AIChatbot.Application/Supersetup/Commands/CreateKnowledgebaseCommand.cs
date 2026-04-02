using MediatR;
using AIChatbot.Application.Common;

public record CreateKnowledgebaseCommand(Guid ConnectionId)
    : IRequest<GenericResult>;