using MediatR;

namespace AIChatbot.Application.RoleAccess.Queries;

public record GetRoleDbAccessQuery(string RoleName)
    : IRequest<List<Guid>>;