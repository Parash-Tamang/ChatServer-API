using MediatR;

namespace AIChatbot.Application.RoleAccess.Queries;

public record GetRoleColumnAccessQuery(
    string RoleName,
    Guid ConnectionId,
    string TableName
) : IRequest<List<string>>;