using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Common;
using AIChatbot.Domain.Entities;
using MediatR;
using AIChatbot.Application.RoleConnectionAccessRetrive.Commands;
namespace AIChatbot.Application.RoleConnectionAccessRetrive.Handlers;

public class SaveRoleInstructionHandler
    : IRequestHandler<SaveRoleInstructionCommand, GenericResult>
{
    private readonly IRoleInstructionRepository _repo;

    public SaveRoleInstructionHandler(
        IRoleInstructionRepository repo)
    {
        _repo = repo;
    }

    public async Task<GenericResult> Handle(
        SaveRoleInstructionCommand request,
        CancellationToken cancellationToken)
    {
        var existing =
            await _repo.GetByRoleIdAsync(request.RoleId);

        if (existing == null)
        {
            await _repo.SaveAsync(new RoleInstruction
            {
                Id = Guid.NewGuid(),
                RoleId = request.RoleId,
                InstructionSet = request.InstructionSet
            });
        }
        else
        {
            existing.InstructionSet = request.InstructionSet;
            existing.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(existing);
        }

        return new GenericResult
        {
            Success = true,
            Message = "Role instruction saved successfully"
        };
    }
}