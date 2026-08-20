using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Common;
using AIChatbot.Domain.Entities;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace AIChatbot.Infrastructure.AI;

public class AiProviderService : IAiProviderService
{
    private readonly HttpClient _http;

    private readonly JsonSerializerOptions
        _jsonOptions;

    private readonly
        IConnectionRepository _connectionRepo;

    private readonly
        IPromptFunctionRepository _promptRepo;

    private readonly
        ILocalFunctionRepository _localRepo;
    private readonly IConnectionExclusionRepository
    _exclusionRepo;

    private readonly ILogger<AiProviderService>
        _logger;

    public AiProviderService(
        HttpClient http,

        IConnectionRepository connectionRepo,

        IPromptFunctionRepository promptRepo,

        ILocalFunctionRepository localRepo,
        IConnectionExclusionRepository exclusionRepo,

        ILogger<AiProviderService> logger)
    {
        _http = http;

        _connectionRepo = connectionRepo;

        _promptRepo = promptRepo;

        _localRepo = localRepo;

        _logger = logger;
        _exclusionRepo = exclusionRepo;

        _jsonOptions =
            new JsonSerializerOptions
            {
                PropertyNamingPolicy =
                    JsonNamingPolicy.CamelCase
            };
    }

    // ============================================================
    // BUILD CONNECTION PAYLOAD
    // ============================================================

    private Dictionary<string, object?> BuildConnectionPayload(
        ConnectionString connection)
    {
        var payload =
            new Dictionary<string, object?>
            {
                ["db_type"] = "mssql",
                ["connection_id"] = connection.Id,
                ["server"] = connection.ServerName.Replace("\\\\", "\\"),
                ["database"] = connection.DatabaseName,
                ["pool_size"] = 5,
                ["timeout"] = connection.ConnectionTimeout
            };

        if (!string.IsNullOrWhiteSpace(connection.Username))
        {
            payload["username"] = connection.Username;
        }

        if (!string.IsNullOrWhiteSpace(connection.PasswordEncrypted))
        {
            payload["password"] = connection.PasswordEncrypted;
        }

        return payload;
    }

    // ============================================================
    // CREATE KNOWLEDGEBASE
    // ============================================================

    public async Task<DatabaseSetupResult>
        PrepareDatabaseAsync(
            ConnectionString connection)
    {
        try
        {
            var exclusions =
                await _exclusionRepo
                    .GetByConnectionAsync(
                        connection.Id)
                ?? Enumerable.Empty<ConnectionExclusion>();
            var excludedSchemas = exclusions
    .Where(x => !x.ExclusionPath.Contains('.'))
    .Select(x => x.ExclusionPath)
    .Distinct()
    .ToList();

            var excludedTables = exclusions
                .Where(x => x.ExclusionPath.Count(c => c == '.') == 1)
                .Select(x => x.ExclusionPath)
                .Distinct()
                .ToList();

            var excludedColumns = exclusions
                .Where(x => x.ExclusionPath.Count(c => c == '.') == 2)
                .Select(x => x.ExclusionPath)
                .Distinct()
                .ToList();

            var payload =
                new Dictionary<string, object?>
                {
                    ["db_type"] = "mssql",
                    ["server"] = connection.ServerName,
                    ["timeout"] = connection.ConnectionTimeout,

                    ["excluded_schemas"] = excludedSchemas,
                    ["excluded_tables"] = excludedTables,
                    ["excluded_columns"] = excludedColumns
                };

            if (!string.IsNullOrWhiteSpace(
                    connection.Username))
            {
                payload["username"] =
                    connection.Username;
            }

            if (!string.IsNullOrWhiteSpace(
                    connection.PasswordEncrypted))
            {
                payload["password"] =
                    connection.PasswordEncrypted;
            }

            var schemaResponse =
                await _http.PostAsJsonAsync(
                    $"knowledgebase/{connection.DatabaseName}/schemas/create",
                    payload,
                    _jsonOptions);

            if (schemaResponse.StatusCode !=
                HttpStatusCode.Created)
            {
                var error =
                    await schemaResponse.Content
                        .ReadAsStringAsync();

                return new DatabaseSetupResult
                {
                    Id = connection.Id,
                    DbStatus = false,
                    Message =
                        $"Knowledgebase creation failed. Schema generation failed. {error}"
                };
            }

            bool viewsCreated = false;

            try
            {
                var viewsResponse =
                    await _http.PostAsJsonAsync(
                        $"knowledgebase/{connection.DatabaseName}/views/create",
                        payload,
                        _jsonOptions);

                viewsCreated =
                    viewsResponse.StatusCode ==
                    HttpStatusCode.Created;
            }
            catch
            {
                viewsCreated = false;
            }

            return new DatabaseSetupResult
            {
                Id = connection.Id,
                DbStatus = true,
                Message = viewsCreated
                    ? "Knowledgebase created successfully. Schema and Views generated."
                    : "Knowledgebase created successfully. Schema generated. Views generation failed."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "PrepareDatabaseAsync failed");

            return new DatabaseSetupResult
            {
                Id = connection.Id,
                DbStatus = false,
                Message = ex.Message
            };
        }
    }
    // ============================================================
    // UPDATE KNOWLEDGEBASE
    // ============================================================

    public async Task<DatabaseSetupResult>
        UpdateDatabaseAsync(
            ConnectionString connection)
    {
        try
        {
            var exclusions =
                await _exclusionRepo
                    .GetByConnectionAsync(
                        connection.Id)
                ?? Enumerable.Empty<ConnectionExclusion>();
            var excludedSchemas = exclusions
    .Where(x => !x.ExclusionPath.Contains('.'))
    .Select(x => x.ExclusionPath)
    .Distinct()
    .ToList();

            var excludedTables = exclusions
                .Where(x => x.ExclusionPath.Count(c => c == '.') == 1)
                .Select(x => x.ExclusionPath)
                .Distinct()
                .ToList();

            var excludedColumns = exclusions
                .Where(x => x.ExclusionPath.Count(c => c == '.') == 2)
                .Select(x => x.ExclusionPath)
                .Distinct()
                .ToList();

            var payload =
    new Dictionary<string, object?>
    {
        ["db_type"] = "mssql",
        ["server"] = connection.ServerName,
        ["timeout"] = connection.ConnectionTimeout,

        ["excluded_schemas"] = excludedSchemas,
        ["excluded_tables"] = excludedTables,
        ["excluded_columns"] = excludedColumns
    };

            if (!string.IsNullOrWhiteSpace(
                    connection.Username))
            {
                payload["username"] =
                    connection.Username;
            }

            if (!string.IsNullOrWhiteSpace(
                    connection.PasswordEncrypted))
            {
                payload["password"] =
                    connection.PasswordEncrypted;
            }

            var schemaResponse =
                await _http.PutAsJsonAsync(
                    $"knowledgebase/{connection.DatabaseName}/schemas/update",
                    payload,
                    _jsonOptions);

            if (!schemaResponse.IsSuccessStatusCode)
            {
                var error =
                    await schemaResponse.Content
                        .ReadAsStringAsync();

                return new DatabaseSetupResult
                {
                    Id = connection.Id,
                    DbStatus = false,
                    Message =
                        $"Knowledgebase update failed. Schema update failed. {error}"
                };
            }

            bool viewsUpdated = false;

            try
            {
                var viewsResponse =
                    await _http.PutAsJsonAsync(
                        $"knowledgebase/{connection.DatabaseName}/views/update",
                        payload,
                        _jsonOptions);

                viewsUpdated =
                    viewsResponse.IsSuccessStatusCode;
            }
            catch
            {
                viewsUpdated = false;
            }

            return new DatabaseSetupResult
            {
                Id = connection.Id,
                DbStatus = true,
                Message = viewsUpdated
                    ? "Knowledgebase updated successfully. Schema and Views updated."
                    : "Knowledgebase updated successfully. Schema updated. Views update failed."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "UpdateDatabaseAsync failed");

            return new DatabaseSetupResult
            {
                Id = connection.Id,
                DbStatus = false,
                Message = ex.Message
            };
        }
    }
    // ============================================================
    // DELETE KNOWLEDGEBASE
    // ============================================================

    public async Task<bool>
        NotifyConnectionDeletedAsync(
            Guid connectionId)
    {
        try
        {
            var connection =
                await _connectionRepo
                    .GetByIdAsync(connectionId);

            if (connection == null)
            {
                return false;
            }

            var response =
                await _http.DeleteAsync(
                    $"knowledgebase/{connection.DatabaseName}/delete");

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Delete knowledgebase failed");

            return false;
        }
    }

    // ============================================================
    // CHAT
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
            if (string.IsNullOrWhiteSpace(userQuery))
            {
                throw new BadHttpRequestException(
                    "Query cannot be empty.");
            }

            var connection =
                await _connectionRepo
                    .GetByIdAsync(connectionId);

            if (connection == null)
            {
                throw new KeyNotFoundException(
                    "Database connection not found.");
            }

            var payload =
                new Dictionary<string, object?>
                {
                    ["user_id"] =
                        userId,

                    ["query"] =
                        userQuery,

                    ["user_role"] =
                        userRole,

                    ["history"] =
                        history?
                            .TakeLast(10)
                            .Select(m => new
                            {
                                role =
                                    m.Role,

                                content =
                                    m.Content
                            })
                            .ToList(),

                    ["session_context"] =
                        sessionContext
                        ??
                        new Dictionary<string, object>(),

                    ["connection_string"] =
                        BuildConnectionPayload(
                            connection),

                    //["model"] =
                    //    "meta-llama/llama-4-scout-17b-16e-instruct"
                };

            Console.WriteLine(
                JsonSerializer.Serialize(payload));

            var response =
                await _http.PostAsJsonAsync(
                    "chat/",
                    payload,
                    _jsonOptions);

            if (!response.IsSuccessStatusCode)
            {
                var errorText =
                    await response.Content
                        .ReadAsStringAsync();

                throw new ApplicationException(
                    $"AI service failed: {errorText}");
            }

            var result =
                await response.Content
                    .ReadFromJsonAsync<JsonElement>(
                        _jsonOptions);





            _logger.LogInformation(
    "RAW AI RESPONSE: {json}",
    result.GetRawText());





            if (!result.TryGetProperty(
                    "success",
                    out var success)
                ||
                success.ValueKind != JsonValueKind.True)
            {
                throw new TimeoutException(
                    "The model failed to respond.");
            }

            var root = result;

            JsonElement dataObj =
                default;

            if (root.TryGetProperty(
                    "data",
                    out var data))
            {
                dataObj = data;
            }

            string? message =
                dataObj.TryGetProperty(
                    "response",
                    out var responseProp)
                        ? responseProp.GetString()
                        : null;

            // ===========================================
            // SESSION CONTEXT
            // ===========================================

            JsonElement sessionObj = default;

            bool hasSessionContext =
                dataObj.TryGetProperty(
                    "session_context",
                    out var sessionContextObj)
                &&
                sessionContextObj.ValueKind ==
                    JsonValueKind.Object;

            if (hasSessionContext)
            {
                sessionObj = sessionContextObj;
            }

            // ===========================================
            // GRAPH
            // ===========================================

            JsonElement graphObj = default;

            bool hasGraph =
                dataObj.TryGetProperty(
                    "graph",
                    out var graph)
                &&
                graph.ValueKind ==
                    JsonValueKind.Object;

            if (hasGraph)
            {
                graphObj = graph;
            }

            // ===========================================
            // EXCEL
            // ===========================================

            JsonElement excelObj = default;

            bool excelGenerated = false;

            if (dataObj.TryGetProperty(
                    "excel",
                    out excelObj)
                &&
                excelObj.ValueKind == JsonValueKind.Object)
            {
                excelGenerated =
                    excelObj.TryGetProperty(
                        "format",
                        out var format)
                    &&
                    string.Equals(
                        format.GetString(),
                        "EXCEL",
                        StringComparison.OrdinalIgnoreCase);
            }
            return new LlmResponse
            {
                Success = true,

                Query = userQuery,

                Message = message,

                // ===========================================
                // SQL
                // ===========================================

                SqlGenerated =
                    hasSessionContext &&
                    sessionObj.TryGetProperty(
                        "last_confirmed_sql",
                        out var sql)
                            ? sql.GetString()
                            : null,

                // ===========================================
                // LEGACY TABLE DATA
                // ===========================================

                Columns =
                    dataObj.TryGetProperty(
                        "columns",
                        out var cols)
                            ? cols.GetRawText()
                            : null,

                Rows =
                    dataObj.TryGetProperty(
                        "rows",
                        out var rows)
                            ? rows.GetRawText()
                            : null,

                RowCount =
                    dataObj.TryGetProperty(
                        "row_count",
                        out var rowCount)
                            ? rowCount.GetInt32()
                            : 0,

                // ===========================================
                // SESSION CONTEXT
                // ===========================================

                RefinedQuery =
                    hasSessionContext &&
                    sessionObj.TryGetProperty(
                        "last_refined_query",
                        out var refined)
                            ? refined.GetString()
                            : null,

                IntentDetected =
                    hasSessionContext &&
                    sessionObj.TryGetProperty(
                        "last_intent",
                        out var intent)
                            ? intent.GetString()
                            : null,

                TablesUsed =
                    hasSessionContext &&
                    sessionObj.TryGetProperty(
                        "last_tables_used",
                        out var tables)
                            ? tables.GetRawText()
                            : null,

                Filters =
                    hasSessionContext &&
                    sessionObj.TryGetProperty(
                        "last_filters",
                        out var filters)
                            ? filters.GetRawText()
                            : null,

                SessionContext =
                    hasSessionContext
                        ? sessionObj.GetRawText()
                        : null,

                // ===========================================
                // GRAPH
                // ===========================================

                GraphType =
                    hasGraph &&
                    graphObj.TryGetProperty(
                        "chart_type",
                        out var chartType)
                            ? chartType.GetString()
                            : null,

                GraphTitle =
                    hasGraph &&
                    graphObj.TryGetProperty(
                        "title",
                        out var title)
                            ? title.GetString()
                            : null,

                GraphReasoning =
                    hasGraph &&
                    graphObj.TryGetProperty(
                        "reasoning",
                        out var reasoning)
                            ? reasoning.GetString()
                            : null,

                GraphImageBase64 =
                    hasGraph &&
                    graphObj.TryGetProperty(
                        "image_base64",
                        out var imageBase64)
                            ? imageBase64.GetString()
                            : null,

                // ===========================================
                // EXCEL
                // ===========================================

                ExcelGenerated =
                    excelGenerated,

                ExcelJson =
                    excelGenerated
                        ? excelObj.GetRawText()
                        : null,

                // ===========================================
                // FLAGS
                // ===========================================

                WasReconstructed =
                    root.TryGetProperty(
                        "was_reconstructed",
                        out var reconstructed)
                    &&
                    reconstructed.ValueKind ==
                        JsonValueKind.True,

                ClarificationNeeded =
                    root.TryGetProperty(
                        "clarification_needed",
                        out var clarification)
                    &&
                    clarification.ValueKind ==
                        JsonValueKind.True,

                TokenUsage =
                    root.TryGetProperty(
                        "token_usage",
                        out var token)
                            ? token.GetRawText()
                            : null,

                ConnectionStringId =
                    connection.Id,

                FullResponseJson =
                    root.GetRawText()
            };
        }
        catch (BadHttpRequestException)
        {
            throw;
        }
        catch (KeyNotFoundException)
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
                "GetReplyAsync failed");

            throw new ApplicationException(
                "System failed to respond.");
        }
    }
}
