using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Common;
using AIChatbot.Application.RoleConnectionAccessRetrive.Commands;
using MediatR;

namespace AIChatbot.Application.RoleConnectionAccessRetrive.Handlers;

public class DeleteConnectionExclusionsHandler
    : IRequestHandler<
        DeleteConnectionExclusionsCommand,
        GenericResult>
{
    private readonly IConnectionExclusionRepository _repo;

    public DeleteConnectionExclusionsHandler(
        IConnectionExclusionRepository repo)
    {
        _repo = repo;
    }

    public async Task<GenericResult> Handle(
        DeleteConnectionExclusionsCommand request,
        CancellationToken cancellationToken)
    {
        await _repo.DeleteByConnectionAsync(
            request.ConnectionId);

        return new GenericResult
        {
            Success = true,
            Message =
                "Connection exclusions deleted successfully"
        };
    }
}