using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Common;
using AIChatbot.Application.RoleAccess.Models;
using AIChatbot.Domain.Entities;
using System.Net.Http.Json;
using System.Text.Json;

namespace AIChatbot.Infrastructure.AI;

public class AiProviderService : IAiProviderService
{
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _jsonOptions;

    public AiProviderService(HttpClient http)
    {
        _http = http;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    // -------------------------------------------------------
    // DATABASE PREPARATION (Supersetup)
    // -------------------------------------------------------
    public async Task<DatabaseSetupResult> PrepareDatabaseAsync(
        ConnectionString connection)
    {
        try
        {
            var payload = new
            {
                server = connection.ServerName.Replace("\\\\", "\\"),
                database_name = connection.DatabaseName,
                auth_mode = connection.AuthMode?.ToLower(),
                username = connection.Username,
                password = connection.PasswordEncrypted,
                trust_certificate = connection.TrustCertificate,
                connection_timeout = connection.ConnectionTimeout,
                db_id = connection.Id.ToString(),
                role = "admin"
            };

            var response = await _http.PostAsJsonAsync(
                "api/knowledgebase/setup",
                payload,
                _jsonOptions);

            if (!response.IsSuccessStatusCode)
            {
                throw new ApplicationException("System was unable to respond to the request");
            }

            var result = await response.Content
                .ReadFromJsonAsync<JsonElement>(_jsonOptions);

            if (result.ValueKind == JsonValueKind.Undefined)
            {
                throw new ApplicationException("System was unable to respond to the request");
            }

            var success = result.TryGetProperty("success", out var s) && s.GetBoolean();

            return new DatabaseSetupResult
            {
                Id = connection.Id,
                DbStatus = success
            };
        }
        catch (Exception)
        {
            throw new ApplicationException("System was unable to respond to the request");
        }
    }

    // -------------------------------------------------------
    // QUERY EXECUTION (Chat)
    // -------------------------------------------------------
    public async Task<LlmResponse> GetReplyAsync(
        string userQuery,
        IEnumerable<Message> history,
        RoleAccessResult schema)
    {
        try
        {
            var conversation = history?
                .TakeLast(5)
                .Select(m => new
                {
                    role = m.Role == "assistant" ? "bot" : m.Role,
                    message = m.Content
                })
                .ToList();

            var payload = new
            {
                query = userQuery,
                conversation_history = conversation,
                save_results = true
            };

            var response = await _http.PostAsJsonAsync(
                "api/query",
                payload,
                _jsonOptions);

            if (!response.IsSuccessStatusCode)
            {
                throw new ApplicationException("System was unable to respond to the request");
            }

            var result = await response.Content
                .ReadFromJsonAsync<JsonElement>(_jsonOptions);

            if (result.ValueKind == JsonValueKind.Undefined)
            {
                throw new ApplicationException("System was unable to respond to the request");
            }

            // 🔴 AI failed to generate response
            if (!result.TryGetProperty("success", out var successProp) ||
                !successProp.GetBoolean())
            {
                throw new TimeoutException("The Model was unable to respond to the request");
            }

            return new LlmResponse
            {
                Success = result.GetProperty("success").GetBoolean(),

                Query = result.TryGetProperty("query", out var q)
                    ? q.GetString() ?? ""
                    : userQuery,

                Message = result.TryGetProperty("message", out var m)
                    ? m.GetString() ?? ""
                    : "",

                SqlGenerated = result.TryGetProperty("sql_generated", out var sql)
                    ? sql.GetString()
                    : null,

                Columns = result.TryGetProperty("columns", out var cols)
                    ? cols.Deserialize<object[]>(_jsonOptions) ?? Array.Empty<object>()
                    : Array.Empty<object>(),

                Rows = result.TryGetProperty("rows", out var rows)
                    ? rows.Deserialize<object[]>(_jsonOptions) ?? Array.Empty<object>()
                    : Array.Empty<object>(),

                RowCount = result.TryGetProperty("row_count", out var rc)
                    ? rc.GetInt32()
                    : 0,

                WasReconstructed = result.TryGetProperty("was_reconstructed", out var wr)
                    && wr.GetBoolean(),

                ClarificationNeeded = result.TryGetProperty("clarification_needed", out var cn)
                    && cn.GetBoolean(),

                TokenUsage = result.TryGetProperty("token_usage", out var tu)
                    ? tu.Deserialize<object>(_jsonOptions) ?? new { }
                    : new { }
            };
        }
        catch (TimeoutException)
        {
            // handled by middleware → 504
            throw;
        }
        catch (Exception)
        {
            // system failure → 500
            throw new ApplicationException("System was unable to respond to the request");
        }
    }
}