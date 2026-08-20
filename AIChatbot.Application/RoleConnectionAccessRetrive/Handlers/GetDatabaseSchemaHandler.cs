using AIChatbot.Application.Abstractions;
using AIChatbot.Application.RoleConnectionAccessRetrive.DTOs;
using AIChatbot.Application.RoleConnectionAccessRetrive.Queries;

using MediatR;

using Microsoft.Data.SqlClient;

namespace AIChatbot.Application.RoleConnectionAccessRetrive.Handlers;

public class GetDatabaseSchemaHandler
    : IRequestHandler<
        GetDatabaseSchemaQuery,
        List<DatabaseSchemaDto>>
{
    private readonly
        IConnectionRepository _connectionRepo;

    public GetDatabaseSchemaHandler(
        IConnectionRepository connectionRepo)
    {
        _connectionRepo = connectionRepo;
    }

    public async Task<List<DatabaseSchemaDto>>
        Handle(
            GetDatabaseSchemaQuery request,
            CancellationToken cancellationToken)
    {
        // ========================================================
        // GET CONNECTION
        // ========================================================

        var connection =
            await _connectionRepo.GetByIdAsync(
                request.ConnectionId);

        if (connection == null)
        {
            throw new Exception(
                "Connection not found");
        }

        // ========================================================
        // BUILD CONNECTION STRING
        // ========================================================

        var connectionString =
            $"Server={connection.ServerName};" +
            $"Database={connection.DatabaseName};" +
            $"Trusted_Connection=True;" +
            $"TrustServerCertificate=True;";

        using var sqlConnection =
            new SqlConnection(connectionString);

        await sqlConnection.OpenAsync(
            cancellationToken);

        var result =
            new List<DatabaseSchemaDto>();

        // ========================================================
        // FETCH TABLES
        // ========================================================

        var tableQuery = @"
            SELECT
                TABLE_SCHEMA,
                TABLE_NAME
            FROM INFORMATION_SCHEMA.TABLES
            WHERE TABLE_TYPE = 'BASE TABLE'
            ORDER BY TABLE_SCHEMA, TABLE_NAME
        ";

        using var tableCmd =
            new SqlCommand(
                tableQuery,
                sqlConnection);

        using var tableReader =
            await tableCmd.ExecuteReaderAsync(
                cancellationToken);

        var tables =
            new List<(string Schema, string Table)>();

        while (await tableReader.ReadAsync(
            cancellationToken))
        {
            tables.Add((
                tableReader.GetString(0),
                tableReader.GetString(1)
            ));
        }

        await tableReader.CloseAsync();

        // ========================================================
        // FETCH COLUMNS
        // ========================================================

        foreach (var item in tables)
        {
            var columnQuery = @"
                SELECT COLUMN_NAME
                FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_SCHEMA = @schema
                AND TABLE_NAME = @table
                ORDER BY ORDINAL_POSITION
            ";

            using var columnCmd =
                new SqlCommand(
                    columnQuery,
                    sqlConnection);

            columnCmd.Parameters.AddWithValue(
                "@schema",
                item.Schema);

            columnCmd.Parameters.AddWithValue(
                "@table",
                item.Table);

            using var columnReader =
                await columnCmd.ExecuteReaderAsync(
                    cancellationToken);

            var columns =
                new List<string>();

            while (await columnReader.ReadAsync(
                cancellationToken))
            {
                columns.Add(
                    columnReader.GetString(0));
            }

            await columnReader.CloseAsync();

            result.Add(new DatabaseSchemaDto
            {
                SchemaName =
                    item.Schema,

                TableName =
                    item.Table,

                Columns =
                    columns
            });
        }

        return result;
    }
}