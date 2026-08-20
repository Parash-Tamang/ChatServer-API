using AIChatbot.Application.Abstractions;
using AIChatbot.Application.RoleConnectionAccessRetrive.Queries;
using MediatR;

namespace AIChatbot.Application.RoleConnectionAccessRetrive.Handlers;

public class GetConnectionExclusionsHandler
    : IRequestHandler<
        GetConnectionExclusionsQuery,
        List<string>>
{
    private readonly IConnectionExclusionRepository _repo;

    public GetConnectionExclusionsHandler(
        IConnectionExclusionRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<string>> Handle(
        GetConnectionExclusionsQuery request,
        CancellationToken cancellationToken)
    {
        var data = await _repo
            .GetByConnectionAsync(
                request.ConnectionId);

        return data
            .Select(x => x.ExclusionPath)
            .ToList();
    }
}