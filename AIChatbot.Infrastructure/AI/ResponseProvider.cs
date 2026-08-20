using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Common;
using AIChatbot.Domain.Entities;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using System.Net.Http.Json;
using System.Text.Json;

namespace AIChatbot.Infrastructure.AI;

public class ResponseProvider
    : IAiProviderService
{
    private readonly HttpClient _http;

    private readonly ILogger<ResponseProvider>
        _logger;

    public ResponseProvider(
        HttpClient http,

        ILogger<ResponseProvider> logger)
    {
        _http = http;

        _logger = logger;
    }

    // ============================================================
    // PREPARE DATABASE
    // ============================================================

    public Task<DatabaseSetupResult>
        PrepareDatabaseAsync(
            ConnectionString connection)
    {
        return Task.FromResult(
            new DatabaseSetupResult
            {
                Id =
                    connection.Id,

                DbStatus =
                    true
            });
    }

    // ============================================================
    // UPDATE DATABASE
    // ============================================================

    public Task<DatabaseSetupResult>
        UpdateDatabaseAsync(
            ConnectionString connection)
    {
        return Task.FromResult(
            new DatabaseSetupResult
            {
                Id =
                    connection.Id,

                DbStatus =
                    true
            });
    }

    // ============================================================
    // DELETE DATABASE
    // ============================================================

    public Task<bool>
        NotifyConnectionDeletedAsync(
            Guid connectionId)
    {
        try
        {
            _logger.LogInformation(
                "Delete connection SUCCESS {Id}",
                connectionId);

            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,

                "Delete connection FAILED {Id}",

                connectionId);

            return Task.FromResult(false);
        }
    }

    // ============================================================
    // CHAT / QUERY EXECUTION
    // ============================================================

    public async Task<LlmResponse>
        GetReplyAsync(
            string userId,

            string userRole,

            string userQuery,

            Guid connectionId,

            IEnumerable<Message> history,

            object? sessionContext = null)
    {
        try
        {
            // ====================================================
            // VALIDATION
            // ====================================================

            if (string.IsNullOrWhiteSpace(
                    userQuery))
            {
                throw new BadHttpRequestException(
                    "Query cannot be empty.");
            }

            // ====================================================
            // HISTORY
            // ====================================================

            var lastMessages =
                history?
                    .TakeLast(10)
                    .Select(m => new
                    {
                        role = m.Role,
                        content = m.Content
                    })
                    .Cast<object>()
                    .ToList()
                ??
                new List<object>();

            // ====================================================
            // PAYLOAD
            // ====================================================

            var payload =
                new Dictionary<string, object?>
                {
                    // ============================================
                    // USER
                    // ============================================

                    ["user_id"] =
                        userId,

                    ["query"] =
                        userQuery,

                    ["user_role"] =
                        userRole,

                    // ============================================
                    // HISTORY
                    // ============================================

                    ["history"] =
                        lastMessages,

                    // ============================================
                    // SESSION
                    // ============================================

                    ["session_context"] =
                        sessionContext,

                    // ============================================
                    // CONNECTION
                    // ============================================

                    ["connection_string"] =
                        new
                        {
                            db_type =
                                "mssql",

                            connection_id =
                                connectionId,

                            // TEMPORARY DUMMY VALUES
                            // Later fetch from DB

                            server =
                                "(localdb)\\MSSQLLocalDB",

                            database =
                                "AdventureWorksLT2019",

                            username =
                                "sa",

                            password =
                                "1234567890",

                            pool_size =
                                5,

                            timeout =
                                30
                        },

                    // ============================================
                    // MODEL
                    // ============================================

                    ["model"] =
                        "meta-llama/llama-4-scout-17b-16e-instruct"
                };

            // ====================================================
            // CALL PYTHON
            // ====================================================

            var response =
                await _http.PostAsJsonAsync(
                    "api/query",
                    payload);

            // ====================================================
            // HTTP VALIDATION
            // ====================================================

            if (!response.IsSuccessStatusCode)
            {
                var error =
                    await response.Content
                        .ReadAsStringAsync();

                _logger.LogError(
                    "Python API FAILED: {Error}",
                    error);

                throw new ApplicationException(
                    "AI provider failed.");
            }

            // ====================================================
            // RAW RESPONSE
            // ====================================================

            var raw =
                await response.Content
                    .ReadAsStringAsync();

            using var doc =
                JsonDocument.Parse(raw);

            var root =
                doc.RootElement;

            // ====================================================
            // SUCCESS
            // ====================================================

            bool success =
                root.TryGetProperty(
                    "success",
                    out var successProp)
                &&
                successProp.GetBoolean();

            // ====================================================
            // MESSAGE
            // ====================================================

            string? message =
                root.TryGetProperty(
                    "message",
                    out var messageProp)
                    ? messageProp.GetString()
                    : null;

            // ====================================================
            // SQL
            // ====================================================

            string? sqlGenerated =
                root.TryGetProperty(
                    "sql_generated",
                    out var sqlProp)
                    ? sqlProp.GetString()
                    : null;

            // ====================================================
            // ROW COUNT
            // ====================================================

            int rowCount =
                root.TryGetProperty(
                    "row_count",
                    out var rowProp)
                    ? rowProp.GetInt32()
                    : 0;

            // ====================================================
            // COLUMNS
            // ====================================================

            object[] columns =
                root.TryGetProperty(
                    "columns",
                    out var colsProp)
                    ? colsProp.Deserialize<object[]>()
                        ??
                        Array.Empty<object>()
                    : Array.Empty<object>();

            // ====================================================
            // ROWS
            // ====================================================

            object[] rows =
                root.TryGetProperty(
                    "rows",
                    out var rowsProp)
                    ? rowsProp.Deserialize<object[]>()
                        ??
                        Array.Empty<object>()
                    : Array.Empty<object>();

            // ====================================================
            // RETURN
            // ====================================================

            return new LlmResponse
            {
                Success =
                    success,

                Query =
                    userQuery,

                Message =
                    message,

                SqlGenerated =
                    sqlGenerated,

                RowCount =
                    rowCount,

                Columns =
                    columns,

                Rows =
                    rows,

                WasReconstructed =
                    false,

                ClarificationNeeded =
                    false,

                TokenUsage =
                    new
                    {
                        prompt = 0,

                        completion = 0
                    }
            };
        }
        catch (BadHttpRequestException)
        {
            throw;
        }
        catch (TimeoutException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,

                "ResponseProvider failed");

            throw new ApplicationException(
                "AI service failed.");
        }
    }
}