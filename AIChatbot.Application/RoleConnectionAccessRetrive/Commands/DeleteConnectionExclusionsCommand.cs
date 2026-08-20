using AIChatbot.Application.Common;
using MediatR;

namespace AIChatbot.Application.RoleConnectionAccessRetrive.Commands;

public record DeleteConnectionExclusionsCommand(
    Guid ConnectionId)
    : IRequest<GenericResult>;