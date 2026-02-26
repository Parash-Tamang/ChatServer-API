using AIChatbot.Application.Abstractions;
using AIChatbot.Application.RoleAccess.Queries;
using MediatR;

namespace AIChatbot.Application.RoleAccess.Handlers;

public class GetRoleDbAccessHandler
    : IRequestHandler<GetRoleDbAccessQuery, List<Guid>>
{
    private readonly IRolePermissionRepository _repo;

    public GetRoleDbAccessHandler(IRolePermissionRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<Guid>> Handle(GetRoleDbAccessQuery request, CancellationToken ct)
    {
        return await _repo.GetDbPermissionsAsync(request.RoleName);
    }
}