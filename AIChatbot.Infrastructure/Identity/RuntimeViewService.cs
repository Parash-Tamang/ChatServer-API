using AIChatbot.Application.Abstractions;
using Microsoft.Data.SqlClient;

namespace AIChatbot.Infrastructure.Identity;

public class RuntimeViewService
    : IRuntimeViewService
{
    private readonly
        IConnectionRepository _connectionRepo;
    private readonly
    IRoleRuntimePermissionRepository
    _permissionRepo;

    private readonly
        IRuntimeViewQueryBuilder
        _queryBuilder;
    public RuntimeViewService(
        IConnectionRepository connectionRepo,

        IRoleRuntimePermissionRepository permissionRepo,

        IRuntimeViewQueryBuilder queryBuilder)
    {
        _connectionRepo = connectionRepo;

        _permissionRepo = permissionRepo;

        _queryBuilder = queryBuilder;
    }

    public async Task<
     (string ViewName, string ViewSql)>
     CreateViewAsync(
         string roleId,
        Guid connectionId)
    {
        var connection =
            await _connectionRepo.GetByIdAsync(
                connectionId);

        if (connection == null)
        {
            throw new Exception(
                "Connection not found.");
        }

        var connectionString =
            $"Server={connection.ServerName};" +
            $"Database={connection.DatabaseName};" +
            $"Trusted_Connection=True;" +
            $"TrustServerCertificate=True;";

        var safeRole =
            roleId.Replace("-", "_");

        var safeConnection =
            connectionId
                .ToString("N");

        var viewName =
            $"vw_runtime_{safeRole}_{safeConnection}";

        var permissions =
            await _permissionRepo.GetAsync(
                roleId,
                connectionId);

        var sql =
            _queryBuilder.Build(
                viewName,
                permissions);

        using var db =
            new SqlConnection(connectionString);

        await db.OpenAsync();

        // ============================================================
        // DROP OLD IF EXISTS
        // ============================================================

        var dropSql = $@"
IF OBJECT_ID('{viewName}', 'V') IS NOT NULL
DROP VIEW [{viewName}]
";

        using (var dropCmd =
            new SqlCommand(dropSql, db))
        {
            await dropCmd.ExecuteNonQueryAsync();
        }

        // ============================================================
        // CREATE NEW VIEW
        // ============================================================

        using var createCmd =
            new SqlCommand(sql, db);

        await createCmd.ExecuteNonQueryAsync();

        return (viewName, sql);
    }

    public async Task DeleteViewAsync(
        Guid connectionId,
        string viewName)
    {
        var connection =
            await _connectionRepo.GetByIdAsync(
                connectionId);

        if (connection == null)
        {
            return;
        }

        var connectionString =
            $"Server={connection.ServerName};" +
            $"Database={connection.DatabaseName};" +
            $"Trusted_Connection=True;" +
            $"TrustServerCertificate=True;";

        using var db =
            new SqlConnection(connectionString);

        await db.OpenAsync();

        var sql = $@"
IF OBJECT_ID('{viewName}', 'V') IS NOT NULL
DROP VIEW [{viewName}]
";

        using var cmd =
            new SqlCommand(sql, db);

        await cmd.ExecuteNonQueryAsync();
    }
}