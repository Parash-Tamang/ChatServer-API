using AIChatbot.Application.Abstractions;
using AIChatbot.Application.RoleAccess.Queries;
using MediatR;

namespace AIChatbot.Application.RoleAccess.Handlers;

public class GetRoleTableAccessHandler
    : IRequestHandler<GetRoleTableAccessQuery, List<string>>
{
    private readonly IRolePermissionRepository _repo;

    public GetRoleTableAccessHandler(IRolePermissionRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<string>> Handle(GetRoleTableAccessQuery request, CancellationToken ct)
    {
        return await _repo.GetTablePermissionsAsync(
            request.RoleName,
            request.ConnectionId);
    }
}