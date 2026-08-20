using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Common;
using AIChatbot.Domain.Entities;
using Dapper;
using Microsoft.Data.SqlClient;

namespace AIChatbot.Infrastructure.Identity;

public class ExternalUserLookupService
    : IExternalUserLookupService
{
    public async Task<ExternalUserLookupResult?> FindUserAsync(
        ConnectionString connection,
        RoleConnectionUserLookupConfiguration config,
        string email,
        string phone)
    {
        var connectionString =
            $"Server={connection.ServerName};" +
            $"Database={connection.DatabaseName};" +
            $"Trusted_Connection=True;" +
            $"TrustServerCertificate=True;";

        using var sql = new SqlConnection(connectionString);

        await sql.OpenAsync();

        // ============================================================
        // CONFIGURED LOOKUP
        // ============================================================

        if (!string.IsNullOrWhiteSpace(config.UserTableName))
        {
            return await ConfiguredLookup(
                 sql,
                 config,
         email,
     phone);
        }

        // ============================================================
        // FALLBACK LIKE SCAN
        // ============================================================

        return await FallbackLookup(
            sql,
            email,
            phone);
    }

    private async Task<ExternalUserLookupResult?>
     ConfiguredLookup(
         SqlConnection sql,
         RoleConnectionUserLookupConfiguration config,
         string email,
         string phone)
    {
        if (string.IsNullOrWhiteSpace(
                config.UserIdColumn))
        {
            throw new Exception(
                "User ID column configuration missing.");
        }

        // ============================================================
        // EMAIL FIRST, THEN PHONE FALLBACK
        // ============================================================

        string? matchColumn = null;
        string? matchValue = null;
        DynamicParameters? parameters = null;

        if (!string.IsNullOrWhiteSpace(email) &&
            !string.IsNullOrWhiteSpace(
                config.EmailColumn))
        {
            matchColumn = config.EmailColumn;
            matchValue = email;
            parameters = new DynamicParameters();
            parameters.Add("@matchValue", email);
        }
        else if (!string.IsNullOrWhiteSpace(phone) &&
            !string.IsNullOrWhiteSpace(
                config.PhoneColumn))
        {
            matchColumn = config.PhoneColumn;
            matchValue = phone;
            parameters = new DynamicParameters();
            parameters.Add("@matchValue", phone);
        }

        if (matchColumn == null ||
            matchValue == null ||
            parameters == null)
        {
            throw new Exception(
                "No valid lookup fields configured.");
        }

        // ============================================================
        // TABLE
        // ============================================================

        string tableSql;

        if (config.UserTableName.Contains('.'))
        {
            var parts =
                config.UserTableName.Split('.');

            tableSql =
                $"[{parts[0]}].[{parts[1]}]";
        }
        else
        {
            tableSql =
                $"[{config.UserTableName}]";
        }

        // ============================================================
        // QUERY
        // ============================================================

        var query = $@"
SELECT TOP 1
    [{config.UserIdColumn}] AS UserId
FROM
    {tableSql}
WHERE
    [{matchColumn}] = @matchValue
";

        Console.WriteLine(query);

        var result =
            await sql.QueryFirstOrDefaultAsync<dynamic>(
                query,
                parameters);

        if (result == null)
        {
            throw new UnauthorizedAccessException(
                "User details did not match database records.");
        }

        return new ExternalUserLookupResult
        {
            Found = true,

            TableName =
                config.UserTableName,

            UserId =
                result.UserId?.ToString() ?? "",

            MatchColumn =
                matchColumn,

            MatchValue =
                matchValue
        };
    }

    private async Task<ExternalUserLookupResult?>
        FallbackLookup(
            SqlConnection sql,
            string email,
            string phone)
    {
        var columns = await sql.QueryAsync<dynamic>(@"
            SELECT
                TABLE_SCHEMA,
                TABLE_NAME,
                COLUMN_NAME
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE
                COLUMN_NAME LIKE '%email%'
                OR COLUMN_NAME LIKE '%mail%'
                OR COLUMN_NAME LIKE '%phone%'
                OR COLUMN_NAME LIKE '%mobile%'
        ");

        foreach (var col in columns)
        {
            string schema = col.TABLE_SCHEMA;
            string table = col.TABLE_NAME;
            string column = col.COLUMN_NAME;

            try
            {
                var idColumn =
                    await sql.QueryFirstOrDefaultAsync<string>(@"
                        SELECT TOP 1 COLUMN_NAME
                        FROM INFORMATION_SCHEMA.COLUMNS
                        WHERE TABLE_SCHEMA = @schema
                        AND TABLE_NAME = @table
                        AND (
                            COLUMN_NAME = 'Id'
                            OR COLUMN_NAME LIKE '%Id%'
                        )
                    ",
                    new
                    {
                        schema,
                        table
                    });

                if (string.IsNullOrWhiteSpace(idColumn))
                    continue;

                var query = $@"
                    SELECT TOP 1
                        [{idColumn}] AS UserId,
                        [{column}] AS MatchValue
                    FROM [{schema}].[{table}]
                    WHERE
                        [{column}] = @email
                        OR [{column}] = @phone
                ";

                var result =
                    await sql.QueryFirstOrDefaultAsync<dynamic>(
                        query,
                        new
                        {
                            email,
                            phone
                        });

                if (result == null)
                    continue;

                return new ExternalUserLookupResult
                {
                    Found = true,

                    TableName = $"{schema}.{table}",

                    UserId = result.UserId?.ToString() ?? "",

                    MatchColumn = column,

                    MatchValue =
                        result.MatchValue?.ToString() ?? ""
                };
            }
            catch
            {
            }
        }

        return null;
    }
}