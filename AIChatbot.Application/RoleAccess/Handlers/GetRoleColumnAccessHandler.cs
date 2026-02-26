using AIChatbot.Application.Abstractions;
using AIChatbot.Application.RoleAccess.Queries;
using MediatR;

namespace AIChatbot.Application.RoleAccess.Handlers;

public class GetRoleColumnAccessHandler
    : IRequestHandler<GetRoleColumnAccessQuery, List<string>>
{
    private readonly IRolePermissionRepository _repo;

    public GetRoleColumnAccessHandler(IRolePermissionRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<string>> Handle(GetRoleColumnAccessQuery request, CancellationToken ct)
    {
        return await _repo.GetColumnPermissionsAsync(
            request.RoleName,
            request.ConnectionId,
            request.TableName);
    }
}