using AIChatbot.Application.Common;
using MediatR;

namespace AIChatbot.Application.RoleConnectionAccessRetrive.Commands;

public record SaveRoleInstructionCommand(
    string RoleId,
    string InstructionSet)
    : IRequest<GenericResult>;