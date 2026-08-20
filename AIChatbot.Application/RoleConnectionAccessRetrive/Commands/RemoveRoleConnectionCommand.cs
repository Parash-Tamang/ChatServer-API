using AIChatbot.Application.Common;
using MediatR;

namespace AIChatbot.Application.RoleConnectionAccessRetrive.Commands;

public record RemoveRoleConnectionCommand(
    string RoleId,
    Guid ConnectionId)
    : IRequest<GenericResult>;