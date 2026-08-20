using AIChatbot.Application.Abstractions;
using AIChatbot.Application.RoleManagement.Queries;
using AIChatbot.Application.RoleManagement.Results;

using MediatR;

using Microsoft.AspNetCore.Identity;

namespace AIChatbot.Application.RoleManagement.Handlers;

public class GetRolesByConnectionHandler
    : IRequestHandler<
        GetRolesByConnectionQuery,
        List<RoleLookupDto>>
{
    private readonly IRoleConnectionMappingRepository
        _mappingRepo;

    private readonly RoleManager<IdentityRole>
        _roleManager;

    public GetRolesByConnectionHandler(
        IRoleConnectionMappingRepository mappingRepo,
        RoleManager<IdentityRole> roleManager)
    {
        _mappingRepo = mappingRepo;
        _roleManager = roleManager;
    }

    public async Task<List<RoleLookupDto>> Handle(
        GetRolesByConnectionQuery request,
        CancellationToken ct)
    {
        var mappings =
            await _mappingRepo
                .GetByConnectionIdAsync(
                    request.ConnectionId);

        var result =
            mappings
                .Select(x => new RoleLookupDto
                {
                    RoleId =
                        x.RoleId,

                    RoleName =
                        x.Role?.Name
                        ?? "Unknown"
                })
                .DistinctBy(x => x.RoleId)
                .ToList();

        return result;
    }
}