using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Common;
using AIChatbot.Application.RoleConnectionAccessRetrive.Commands;
using MediatR;

namespace AIChatbot.Application.RoleConnectionAccessRetrive.Handlers;

public class DeleteRoleInstructionHandler
    : IRequestHandler<
        DeleteRoleInstructionCommand,
        GenericResult>
{
    private readonly IRoleInstructionRepository _repo;

    public DeleteRoleInstructionHandler(
        IRoleInstructionRepository repo)
    {
        _repo = repo;
    }

    public async Task<GenericResult> Handle(
        DeleteRoleInstructionCommand request,
        CancellationToken cancellationToken)
    {
        await _repo.DeleteAsync(request.RoleId);

        return new GenericResult
        {
            Success = true,
            Message = "Role instruction deleted successfully"
        };
    }
}