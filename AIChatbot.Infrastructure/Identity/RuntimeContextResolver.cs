using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Common;
using Dapper;
using Microsoft.Data.SqlClient;

namespace AIChatbot.Infrastructure.Identity;

public class RuntimeContextResolver
    : IRuntimeContextResolver
{
    private readonly
        IExternalUserMappingRepository
        _externalRepo;

    private readonly
        IRoleRuntimePermissionRepository
        _permissionRepo;

    private readonly
        IConnectionRepository
        _connectionRepo;

    public RuntimeContextResolver(
        IExternalUserMappingRepository externalRepo,

        IRoleRuntimePermissionRepository permissionRepo,

        IConnectionRepository connectionRepo)
    {
        _externalRepo = externalRepo;

        _permissionRepo = permissionRepo;

        _connectionRepo = connectionRepo;
    }

    public async Task<RuntimeContextResult>
        ResolveAsync(
            string applicationUserId,
            string roleId,
            Guid connectionId)
    {
        var result = new RuntimeContextResult();

        // ============================================================
        // GET EXTERNAL USER
        // ============================================================

        var externalUser =
            await _externalRepo.GetAsync(
                applicationUserId,
                connectionId);
        if (externalUser == null)
        {
            return result;
        }

        // ============================================================
        // GET PERMISSIONS
        // ============================================================

        var permissions =
            await _permissionRepo.GetAsync(
                roleId,
                connectionId);

        var runtimePermissions =
            permissions
                .Where(x =>
                    !string.IsNullOrWhiteSpace(x.RuntimeKey) &&
                    (x.FilterType == "id" || x.FilterType == "runtime_id"))
                .ToList();

        if (runtimePermissions.Count == 0)
        {
            return result;
        }

        // ============================================================
        // GET CONNECTION
        // ============================================================

        // var connection =
        //     await _connectionRepo.GetByIdAsync(
        //         connectionId);

        // if (connection == null)
        // {
        //     throw new Exception(
        //         "Connection not found.");
        // }

        // var connectionString =
        //     $"Server={connection.ServerName};" +
        //     $"Database={connection.DatabaseName};" +
        //     $"Trusted_Connection=True;" +
        //     $"TrustServerCertificate=True;";

        // using var sql =
        //     new SqlConnection(connectionString);

        // await sql.OpenAsync();

        // ============================================================
        // BUILD RUNTIME VALUES
        // ============================================================

        foreach (var permission in runtimePermissions)
        {
            var key =
                permission.RuntimeKey!;

            if (result.Values.ContainsKey(key))
            {
                continue;
            }

            // var query = $@"
            //     SELECT DISTINCT
            //         [{permission.ColumnName}]
            //     FROM
            //         [{permission.SchemaName}].
            //         [{permission.TableName}]
            //     WHERE
            //         [{permission.ColumnName}] = @id
            // ";

            // var values =
            //     (await sql.QueryAsync<string>(
            //         query,
            //         new
            //         {
            //             id = externalUser.ExternalUserId
            //         }))
            //     .Distinct()
            //     .ToList();

            // Console.WriteLine($"Key: {key}");
            // Console.WriteLine($"Values: {string.Join(", ", values)}");
            // Console.WriteLine($"Query: {externalUser.ExternalUserId}");
           
            result.Values[key] = new List<string> {externalUser.ExternalUserId};
        }

        return result;
    }
}