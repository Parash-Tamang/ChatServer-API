using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Common;
using AIChatbot.Application.RoleConnectionAccessRetrive.Commands;
using AIChatbot.Domain.Entities;
using MediatR;

namespace AIChatbot.Application.RoleConnectionAccessRetrive.Handlers;

public class SaveUserLookupConfigurationHandler
    : IRequestHandler<
        SaveUserLookupConfigurationCommand,
        GenericResult>
{
    private readonly
        IRoleConnectionUserLookupConfigurationRepository
        _repo;

    private readonly
        IRoleConnectionMappingRepository
        _mappingRepo;

    public SaveUserLookupConfigurationHandler(
        IRoleConnectionUserLookupConfigurationRepository repo,

        IRoleConnectionMappingRepository mappingRepo)
    {
        _repo = repo;
        _mappingRepo = mappingRepo;
    }

    public async Task<GenericResult> Handle(
        SaveUserLookupConfigurationCommand request,
        CancellationToken cancellationToken)
    {
        // ============================================================
        // VALIDATE ROLE ↔ DB ASSIGNMENT
        // ============================================================

        var assigned =
            await _mappingRepo.ExistsAsync(
                request.RoleId,
                request.ConnectionId);

        if (!assigned)
        {
            throw new UnauthorizedAccessException(
                "Database is not assigned to role.");
        }

        var existing =
            await _repo.GetAsync(
                request.RoleId,
                request.ConnectionId);

        if (existing == null)
        {
            await _repo.SaveAsync(
                new RoleConnectionUserLookupConfiguration
                {
                    Id = Guid.NewGuid(),

                    RoleId = request.RoleId,

                    ConnectionId = request.ConnectionId,

                    UserTableName =
                        request.UserTableName,

                    UserIdColumn =
                        request.UserIdColumn,

                    EmailColumn =
                        request.EmailColumn,

                    PhoneColumn =
                        request.PhoneColumn,

                    CreatedAt = DateTime.UtcNow
                });
        }
        else
        {
            existing.UserTableName =
                request.UserTableName;

            existing.UserIdColumn =
                request.UserIdColumn;

            existing.EmailColumn =
                request.EmailColumn;

            existing.PhoneColumn =
                request.PhoneColumn;

            await _repo.UpdateAsync(existing);
        }

        return new GenericResult
        {
            Success = true,
            Message =
                "User lookup configuration saved successfully"
        };
    }
}