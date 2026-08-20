using AIChatbot.Application.Common;
using MediatR;

namespace AIChatbot.Application.RoleConnectionAccessRetrive.Commands;

public record DeleteRoleInstructionCommand(
    string RoleId)
    : IRequest<GenericResult>;