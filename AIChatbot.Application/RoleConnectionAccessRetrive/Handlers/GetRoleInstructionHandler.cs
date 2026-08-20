using AIChatbot.Application.Abstractions;
using AIChatbot.Application.RoleConnectionAccessRetrive.Queries;
using MediatR;

namespace AIChatbot.Application.RoleConnectionAccessRetrive.Handlers;

public class GetRoleInstructionHandler
    : IRequestHandler<GetRoleInstructionQuery, string?>
{
    private readonly IRoleInstructionRepository _repo;

    public GetRoleInstructionHandler(
        IRoleInstructionRepository repo)
    {
        _repo = repo;
    }

    public async Task<string?> Handle(
        GetRoleInstructionQuery request,
        CancellationToken cancellationToken)
    {
        var data = await _repo.GetByRoleIdAsync(request.RoleId);

        return data?.InstructionSet;
    }
}