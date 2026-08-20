using AIChatbot.Application.Common;
using AIChatbot.Application.RoleConnectionAccessRetrive.DTOs;
using MediatR;

namespace AIChatbot.Application.RoleConnectionAccessRetrive.Commands;

public record SaveRuntimePermissionCommand(
    string Role,
    string SelectedRoleId,
    string Database,
    Guid ConnectionId,
    Dictionary<string, RuntimeTablePermissionDto> Permissions)
    : IRequest<GenericResult>;