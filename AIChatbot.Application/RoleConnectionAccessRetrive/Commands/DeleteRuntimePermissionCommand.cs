using AIChatbot.Application.Common;
using MediatR;

namespace AIChatbot.Application.RoleConnectionAccessRetrive.Commands;

public record DeleteRuntimePermissionCommand(
    string RoleId,
    Guid ConnectionId)
    : IRequest<GenericResult>;