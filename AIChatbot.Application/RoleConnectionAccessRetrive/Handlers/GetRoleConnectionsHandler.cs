using AIChatbot.Application.Abstractions;
using AIChatbot.Application.RoleConnectionAccessRetrive.DTOs;
using AIChatbot.Application.RoleConnectionAccessRetrive.Queries;
using MediatR;

namespace AIChatbot.Application.RoleConnectionAccessRetrive.Handlers;

public class GetRoleConnectionsHandler
    : IRequestHandler<
        GetRoleConnectionsQuery,
        List<RoleConnectionDto>>
{
    private readonly IRoleConnectionMappingRepository _repo;

    public GetRoleConnectionsHandler(
        IRoleConnectionMappingRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<RoleConnectionDto>> Handle(
        GetRoleConnectionsQuery request,
        CancellationToken cancellationToken)
    {
        var mappings =
            await _repo.GetByRoleIdAsync(
                request.RoleId);

        return mappings
            .Select(x =>
                new RoleConnectionDto
                {
                    RoleId =
                        x.RoleId,

                    RoleName =
                        x.Role?.Name ?? string.Empty,

                    ConnectionId =
                        x.ConnectionId,

                    ServerName =
                        x.Connection?.ServerName
                        ?? string.Empty,

                    DatabaseName =
                        x.Connection?.DatabaseName
                        ?? string.Empty,

                    AuthMode =
                        x.Connection?.AuthMode
                        ?? string.Empty
                })
            .ToList();
    }
}