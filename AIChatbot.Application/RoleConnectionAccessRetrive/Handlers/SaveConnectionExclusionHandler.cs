using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Common;
using AIChatbot.Application.RoleConnectionAccessRetrive.Commands;
using AIChatbot.Domain.Entities;
using MediatR;

namespace AIChatbot.Application.RoleConnectionAccessRetrive.Handlers;

public class SaveConnectionExclusionHandler
    : IRequestHandler<
        SaveConnectionExclusionCommand,
        GenericResult>
{
    private readonly IConnectionExclusionRepository _repo;

    public SaveConnectionExclusionHandler(
        IConnectionExclusionRepository repo)
    {
        _repo = repo;
    }

    public async Task<GenericResult> Handle(
        SaveConnectionExclusionCommand request,
        CancellationToken cancellationToken)
    {
        // ============================================================
        // DELETE EXISTING
        // ============================================================

        await _repo.DeleteByConnectionAsync(
            request.ConnectionId);

        var entities = new List<ConnectionExclusion>();

        foreach (var schema in request.Exclusions)
        {
            // ========================================================
            // SCHEMA LEVEL
            // ========================================================

            if (schema.Tables == null ||
                schema.Tables.Count == 0)
            {
                entities.Add(new ConnectionExclusion
                {
                    Id = Guid.NewGuid(),

                    ConnectionId =
                        request.ConnectionId,

                    ExclusionPath =
                        schema.SchemaName,

                    CreatedAt =
                        DateTime.UtcNow
                });

                continue;
            }

            foreach (var table in schema.Tables)
            {
                // ====================================================
                // TABLE LEVEL
                // ====================================================

                if (table.Columns == null ||
                    table.Columns.Count == 0)
                {
                    entities.Add(new ConnectionExclusion
                    {
                        Id = Guid.NewGuid(),

                        ConnectionId =
                            request.ConnectionId,

                        ExclusionPath =
                            $"{schema.SchemaName}.{table.TableName}",

                        CreatedAt =
                            DateTime.UtcNow
                    });

                    continue;
                }

                // ====================================================
                // COLUMN LEVEL
                // ====================================================

                foreach (var column in table.Columns)
                {
                    entities.Add(new ConnectionExclusion
                    {
                        Id = Guid.NewGuid(),

                        ConnectionId =
                            request.ConnectionId,

                        ExclusionPath =
                            $"{schema.SchemaName}.{table.TableName}.{column}",

                        CreatedAt =
                            DateTime.UtcNow
                    });
                }
            }
        }

        await _repo.SaveAsync(entities);

        return new GenericResult
        {
            Success = true,
            Message =
                "Connection exclusions saved successfully"
        };
    }
}