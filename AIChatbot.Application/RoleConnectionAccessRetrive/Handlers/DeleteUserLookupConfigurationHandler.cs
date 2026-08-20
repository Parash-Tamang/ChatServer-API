using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Common;
using AIChatbot.Application.RoleConnectionAccessRetrive.Commands;
using MediatR;

namespace AIChatbot.Application.RoleConnectionAccessRetrive.Handlers;

public class DeleteUserLookupConfigurationHandler
    : IRequestHandler<
        DeleteUserLookupConfigurationCommand,
        GenericResult>
{
    private readonly
        IRoleConnectionUserLookupConfigurationRepository
        _repo;

    public DeleteUserLookupConfigurationHandler(
        IRoleConnectionUserLookupConfigurationRepository repo)
    {
        _repo = repo;
    }

    public async Task<GenericResult> Handle(
        DeleteUserLookupConfigurationCommand request,
        CancellationToken cancellationToken)
    {
        await _repo.DeleteAsync(
            request.RoleId,
            request.ConnectionId);

        return new GenericResult
        {
            Success = true,
            Message =
                "User lookup configuration deleted successfully"
        };
    }
}