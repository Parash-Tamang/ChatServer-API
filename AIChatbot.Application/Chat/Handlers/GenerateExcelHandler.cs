using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Chat.Commands;
using AIChatbot.Domain.Entities;
using ClosedXML.Excel;
using MediatR;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.Json;

namespace AIChatbot.Application.Chat.Handlers;

public class GenerateExcelHandler
    : IRequestHandler<GenerateExcelCommand, byte[]>
{
    private readonly IResponseMetadataRepository _metadataRepo;
    private readonly IConnectionRepository _connectionRepo;

    public GenerateExcelHandler(
        IResponseMetadataRepository metadataRepo,
        IConnectionRepository connectionRepo)
    {
        _metadataRepo = metadataRepo;
        _connectionRepo = connectionRepo;
    }

    public async Task<byte[]> Handle(
     GenerateExcelCommand request,
     CancellationToken cancellationToken)
    {
        var metadata =
            await _metadataRepo.GetByMessageIdAsync(
                request.MessageId)
            ?? throw new Exception(
                "Metadata not found.");

        if (string.IsNullOrWhiteSpace(
                metadata.SessionContextJson))
        {
            throw new Exception(
                "Session context not found.");
        }

        // ============================================
        // EXTRACT SQL FROM SESSION CONTEXT
        // ============================================

        string? sql = null;

        using (var doc =
               JsonDocument.Parse(
                   metadata.SessionContextJson))
        {
            var root = doc.RootElement;

            // Old records:
            // "{ \"last_confirmed_sql\":\"...\" }"
            if (root.ValueKind == JsonValueKind.String)
            {
                var innerJson =
                    root.GetString();

                if (!string.IsNullOrWhiteSpace(
                        innerJson))
                {
                    using var innerDoc =
                        JsonDocument.Parse(
                            innerJson);

                    sql =
                        innerDoc.RootElement
                            .GetProperty(
                                "last_confirmed_sql")
                            .GetString();
                }
            }
            else
            {
                // New records:
                // { "last_confirmed_sql":"..." }

                sql =
                    root.GetProperty(
                        "last_confirmed_sql")
                    .GetString();
            }
        }

        if (string.IsNullOrWhiteSpace(sql))
        {
            throw new Exception(
                "No SQL found in session context.");
        }

        // ============================================
        // CONNECTION
        // ============================================

        if (!metadata.ConnectionStringId.HasValue)
        {
            throw new Exception(
                "Connection not found.");
        }

        var connection =
            await _connectionRepo.GetByIdAsync(
                metadata.ConnectionStringId.Value)
            ?? throw new Exception(
                "Connection not found.");

        var sqlConnectionString =
            BuildConnectionString(
                connection);

        // ============================================
        // EXECUTE SQL
        // ============================================

        var table =
            new DataTable();

        using (var sqlConn =
               new SqlConnection(
                   sqlConnectionString))
        {
            await sqlConn.OpenAsync(
                cancellationToken);

            using var cmd =
                new SqlCommand(
                    sql,
                    sqlConn);

            using var reader =
                await cmd.ExecuteReaderAsync(
                    cancellationToken);

            table.Load(reader);
        }

        // ============================================
        // CREATE EXCEL
        // ============================================

        using var workbook =
            new XLWorkbook();

        workbook.Worksheets.Add(
            table,
            "Results");

        using var stream =
            new MemoryStream();

        workbook.SaveAs(stream);

        return stream.ToArray();
    }
    private string BuildConnectionString(
        ConnectionString conn)
    {
        return
            $"Server={conn.ServerName};" +
            $"Database={conn.DatabaseName};" +
            $"User Id={conn.Username};" +
            $"Password={conn.PasswordEncrypted};" +
            $"TrustServerCertificate=True;";
    }
}