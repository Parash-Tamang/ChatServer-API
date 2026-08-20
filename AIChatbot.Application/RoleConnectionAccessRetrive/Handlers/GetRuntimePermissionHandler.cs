using AIChatbot.Application.Abstractions;
using AIChatbot.Application.RoleConnectionAccessRetrive.DTOs;
using AIChatbot.Application.RoleConnectionAccessRetrive.Queries;
using MediatR;
using System.Text.Json;

namespace AIChatbot.Application.RoleConnectionAccessRetrive.Handlers;

public class GetRuntimePermissionHandler
    : IRequestHandler<
        GetRuntimePermissionQuery,
        RuntimePermissionResponseDto?>
{
    private readonly
        IRoleRuntimePermissionRepository _repo;

    private readonly
        IConnectionRepository _connectionRepo;

    private readonly
        IRoleConnectionMappingRepository _mappingRepo;

    public GetRuntimePermissionHandler(
        IRoleRuntimePermissionRepository repo,
        IConnectionRepository connectionRepo,
        IRoleConnectionMappingRepository mappingRepo)
    {
        _repo = repo;
        _connectionRepo = connectionRepo;
        _mappingRepo = mappingRepo;
    }

    public async Task<RuntimePermissionResponseDto?>
        Handle(
            GetRuntimePermissionQuery request,
            CancellationToken cancellationToken)
    {
        var data =
            await _repo.GetAsync(
                request.RoleId,
                request.ConnectionId);

        if (data == null || data.Count == 0)
        {
            return null;
        }

        var mapping =
            await _mappingRepo.GetByRoleIdAsync(
                request.RoleId);

        var roleMapping =
            mapping?.FirstOrDefault(x =>
                x.ConnectionId == request.ConnectionId);

        var connection =
            await _connectionRepo.GetByIdAsync(
                request.ConnectionId);

        var roleName = roleMapping?.Role?.Name ?? string.Empty;

        var groupedPermissions =
            data.GroupBy(x =>
                $"{x.SchemaName}.{x.TableName}");

        var permissions =
            new Dictionary<string, RuntimeTablePermissionDto>();

        foreach (var group in groupedPermissions)
        {
            var tableKey = group.Key;
            var first = group.First();

            var filters =
                new List<RuntimeFilterDto>();

            if (string.Equals(
                    first.AccessLevel,
                    "filtered",
                    StringComparison.OrdinalIgnoreCase))
            {
                foreach (var permission in group)
                {
                    object? values = null;

                    if (!string.IsNullOrWhiteSpace(
                            permission.FilterValuesJson))
                    {
                        values =
                            JsonSerializer.Deserialize<object>(
                                permission.FilterValuesJson);
                    }

                    filters.Add(
                        new RuntimeFilterDto
                        {
                            Column =
                                permission.ColumnName,

                            FilterType =
                                permission.FilterType,

                            Values = values
                        });
                }
            }

            permissions[tableKey] =
                new RuntimeTablePermissionDto
                {
                    Reason = first.Reason,

                    AccessLevel =
                        first.AccessLevel,

                    RequiredFilters =
                        filters
                };
        }

        return new RuntimePermissionResponseDto
        {
            Role = roleName,

            SelectedRoleId = request.RoleId,

            Database = connection?.DatabaseName ?? string.Empty,

            ConnectionId = request.ConnectionId,

            Permissions = permissions
        };
    }
}