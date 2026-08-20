using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Common;
using AIChatbot.Application.RoleConnectionAccessRetrive.Commands;
using MediatR;

namespace AIChatbot.Application.RoleConnectionAccessRetrive.Handlers;

public class RemoveRoleConnectionHandler
    : IRequestHandler<
        RemoveRoleConnectionCommand,
        GenericResult>
{
    private readonly IRoleConnectionMappingRepository _repo;

    public RemoveRoleConnectionHandler(
        IRoleConnectionMappingRepository repo)
    {
        _repo = repo;
    }

    public async Task<GenericResult> Handle(
        RemoveRoleConnectionCommand request,
        CancellationToken cancellationToken)
    {
        await _repo.DeleteAsync(
            request.RoleId,
            request.ConnectionId);

        return new GenericResult
        {
            Success = true,
            Message = "Database removed from role successfully"
        };
    }
}