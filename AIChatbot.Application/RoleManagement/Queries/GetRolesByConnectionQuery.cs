using AIChatbot.Application.RoleManagement.Results;
using MediatR;

namespace AIChatbot.Application.RoleManagement.Queries;

public record GetRolesByConnectionQuery(
    Guid ConnectionId)
    : IRequest<List<RoleLookupDto>>;