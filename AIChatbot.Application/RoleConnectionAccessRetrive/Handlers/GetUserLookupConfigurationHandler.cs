using AIChatbot.Application.Abstractions;
using AIChatbot.Application.RoleConnectionAccessRetrive.Queries;
using AIChatbot.Domain.Entities;
using MediatR;

namespace AIChatbot.Application.RoleConnectionAccessRetrive.Handlers;

public class GetUserLookupConfigurationHandler
    : IRequestHandler<
        GetUserLookupConfigurationQuery,
        RoleConnectionUserLookupConfiguration?>
{
    private readonly
        IRoleConnectionUserLookupConfigurationRepository
        _repo;

    public GetUserLookupConfigurationHandler(
        IRoleConnectionUserLookupConfigurationRepository repo)
    {
        _repo = repo;
    }

    public async Task<RoleConnectionUserLookupConfiguration?>
        Handle(
            GetUserLookupConfigurationQuery request,
            CancellationToken cancellationToken)
    {
        return await _repo.GetAsync(
            request.RoleId,
            request.ConnectionId);
    }
}