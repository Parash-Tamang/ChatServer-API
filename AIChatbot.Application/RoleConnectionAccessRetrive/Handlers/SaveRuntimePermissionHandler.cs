using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Common;
using AIChatbot.Application.RoleConnectionAccessRetrive.Commands;
using AIChatbot.Domain.Entities;
using MediatR;
using System.Text.Json;

namespace AIChatbot.Application.RoleConnectionAccessRetrive.Handlers;

public class SaveRuntimePermissionHandler
    : IRequestHandler<
        SaveRuntimePermissionCommand,
        GenericResult>
{
    private readonly
        IRoleRuntimePermissionRepository _repo;

    private readonly
        IRoleConnectionMappingRepository _mappingRepo;

    private readonly
        IRuntimeViewService _viewService;

    private readonly
        IRuntimePermissionViewRepository _viewRepo;

    public SaveRuntimePermissionHandler(
        IRoleRuntimePermissionRepository repo,

        IRoleConnectionMappingRepository mappingRepo,

        IRuntimeViewService viewService,

        IRuntimePermissionViewRepository viewRepo)
    {
        _repo = repo;

        _mappingRepo = mappingRepo;

        _viewService = viewService;

        _viewRepo = viewRepo;
    }

    public async Task<GenericResult> Handle(
    SaveRuntimePermissionCommand request,
    CancellationToken cancellationToken)
    {
        // ============================================================
        // VALIDATE ROLE ↔ DB ASSIGNMENT
        // ============================================================

        var assigned =
            await _mappingRepo.ExistsAsync(
                request.SelectedRoleId,
                request.ConnectionId);

        if (!assigned)
        {
            throw new UnauthorizedAccessException(
                "Database is not assigned to role.");
        }

        // ============================================================
        // DELETE OLD RUNTIME PERMISSIONS
        // ============================================================

        await _repo.DeleteAsync(
            request.SelectedRoleId,
            request.ConnectionId);

        // ============================================================
        // DELETE OLD VIEW
        // ============================================================

        var existingView =
            await _viewRepo.GetAsync(
                request.SelectedRoleId,
                request.ConnectionId);

        if (existingView != null)
        {
            await _viewService.DeleteViewAsync(
                request.ConnectionId,
                existingView.ViewName);

            await _viewRepo.DeleteAsync(
                existingView);
        }

        // ============================================================
        // BUILD NEW ENTITIES
        // ============================================================

        var entities =
            new List<RoleRuntimePermission>();

        foreach (var tablePermission in request.Permissions)
        {
            var tableKey =
                tablePermission.Key;

            var permission =
                tablePermission.Value;

            var split =
                tableKey.Split('.');

            if (split.Length != 2)
            {
                throw new Exception(
                    $"Invalid table format: {tableKey}. Expected Schema.Table");
            }

            var schemaName =
                split[0];

            var tableName =
                split[1];

            var accessLevel =
                permission.AccessLevel?.Trim()
                ?? string.Empty;

            // ========================================================
            // UNRESTRICTED
            // ========================================================

            if (string.Equals(
         accessLevel,
         "unrestricted",
         StringComparison.OrdinalIgnoreCase))
            {
                entities.Add(
                    new RoleRuntimePermission
                    {
                        Id = Guid.NewGuid(),

                        RoleId = request.SelectedRoleId,

                        ConnectionId = request.ConnectionId,

                        SchemaName = schemaName,

                        TableName = tableName,

                        ColumnName = string.Empty,

                        FilterType = string.Empty,

                        RuntimeKey = null,

                        FilterValuesJson = null,

                        AccessLevel = accessLevel,

                        Reason = permission.Reason,

                        CreatedAt = DateTime.UtcNow
                    });

                continue;
            }

            // ========================================================
            // FILTERED VALIDATION
            // ========================================================

            if (string.Equals(
         accessLevel,
         "filtered",
         StringComparison.OrdinalIgnoreCase))
            {
                foreach (var filter in permission.RequiredFilters)
                {
                    entities.Add(
                        new RoleRuntimePermission
                        {
                            Id = Guid.NewGuid(),

                            RoleId = request.SelectedRoleId,

                            ConnectionId = request.ConnectionId,

                            SchemaName = schemaName,

                            TableName = tableName,

                            ColumnName = filter.Column,

                            FilterType = filter.FilterType,

                            RuntimeKey =
                                filter.FilterType == "id"
                                    ? filter.Column
                                    : null,

                            FilterValuesJson =
                                filter.Values == null
                                    ? null
                                    : JsonSerializer.Serialize(filter.Values),

                            AccessLevel = accessLevel,

                            Reason = permission.Reason,

                            CreatedAt = DateTime.UtcNow
                        });
                }

                continue;
            }
            // ========================================================
            // UNKNOWN ACCESS LEVEL
            // ========================================================

            throw new Exception(
                $"Unsupported access level '{accessLevel}' for table '{tableKey}'.");
        }

        await _repo.SaveAsync(entities);

        // ============================================================
        // CREATE VIEW
        // ============================================================

        var viewResult =
            await _viewService.CreateViewAsync(
                request.SelectedRoleId,
                request.ConnectionId);

        await _viewRepo.AddAsync(
            new RuntimePermissionView
            {
                Id = Guid.NewGuid(),

                RoleId =
                    request.SelectedRoleId,

                ConnectionId =
                    request.ConnectionId,

                ViewName =
                    viewResult.ViewName,

                ViewSql =
                    viewResult.ViewSql,

                CreatedAt =
                    DateTime.UtcNow
            });

        // ============================================================
        // RESPONSE
        // ============================================================

        return new GenericResult
        {
            Success = true,
            Message =
                "Runtime permissions and view saved successfully"
        };
    }
}