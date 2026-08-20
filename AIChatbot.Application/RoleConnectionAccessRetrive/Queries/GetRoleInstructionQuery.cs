using MediatR;

namespace AIChatbot.Application.RoleConnectionAccessRetrive.Queries;

public record GetRoleInstructionQuery(string RoleId)
    : IRequest<string?>;