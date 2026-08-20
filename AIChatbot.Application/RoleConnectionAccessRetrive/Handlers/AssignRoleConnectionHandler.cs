using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Common;
using AIChatbot.Application.RoleConnectionAccessRetrive.Commands;
using AIChatbot.Domain.Entities;
using MediatR;

namespace AIChatbot.Application.RoleConnectionAccessRetrive.Handlers;

public class AssignRoleConnectionHandler
    : IRequestHandler<
        AssignRoleConnectionCommand,
        GenericResult>
{
    private readonly
        IRoleConnectionMappingRepository _repo;

    public AssignRoleConnectionHandler(
        IRoleConnectionMappingRepository repo)
    {
        _repo = repo;
    }

    public async Task<GenericResult> Handle(
        AssignRoleConnectionCommand request,
        CancellationToken cancellationToken)
    {
        // ============================================================
        // PREVENT DUPLICATE
        // ============================================================

        var exists =
            await _repo.ExistsAsync(
                request.RoleId,
                request.ConnectionId);

        if (exists)
        {
            throw new Exception(
                "Database already assigned to role.");
        }

        // ============================================================
        // SAVE
        // ============================================================

        await _repo.AddAsync(
            new RoleConnectionMapping
            {
                Id = Guid.NewGuid(),

                RoleId = request.RoleId,

                ConnectionId = request.ConnectionId,

                CreatedAt = DateTime.UtcNow
            });

        return new GenericResult
        {
            Success = true,
            Message =
                "Database assigned to role successfully"
        };
    }
}