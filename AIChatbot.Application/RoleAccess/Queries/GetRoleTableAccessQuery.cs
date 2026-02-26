using MediatR;

namespace AIChatbot.Application.RoleAccess.Queries;

public record GetRoleTableAccessQuery(
    string RoleName,
    Guid ConnectionId
) : IRequest<List<string>>;