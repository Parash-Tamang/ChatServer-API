using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Common;
using AIChatbot.Application.RoleAccess.Models;
using AIChatbot.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace AIChatbot.Infrastructure.AI;

public class AiProviderService : IAiProviderService
{
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly IConnectionRepository _connectionRepo;
    private readonly ILogger<AiProviderService> _logger;

    public AiProviderService(
        HttpClient http,
        IConnectionRepository connectionRepo,
        ILogger<AiProviderService> logger)
    {
        _http = http;
        _connectionRepo = connectionRepo;
        _logger = logger;

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
                // role = "admin"
            };

            var response = await _http.PostAsJsonAsync(
                "api/knowledgebase/setup",
                payload,
                _jsonOptions);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                _logger.LogError("Knowledgebase/setup failed with {StatusCode}: {Body}", response.StatusCode, body);
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "PrepareDatabaseAsync failed");
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
            // Use strongly-typed dictionaries for predictable JSON serialization
            var conversation = history?
                .TakeLast(5)
                .Select(m => new Dictionary<string, string?>
                {
                    ["message"] = m.Content
                })
                .ToList() ?? new List<Dictionary<string, string?>>();

            // Build payload as dictionary so we can conditionally add DB connection info
            var payload = new Dictionary<string, object?>
            {
                ["query"] = userQuery,
                ["conversation_history"] = conversation,
                ["save_results"] = true
            };

            // Determine a verified connection to send (schema first, then active verified)
            ConnectionString? chosenConnection = null;

            if (schema?.Databases?.Any() == true)
            {
                foreach (var dbId in schema.Databases)
                {
                    var conn = await _connectionRepo.GetByIdAsync(dbId);
                    if (conn != null && conn.Verified)
                    {
                        chosenConnection = conn;
                        break;
                    }
                }
            }

            if (chosenConnection == null)
            {
                // Fallback: look for an active verified connection
                try
                {
                    var all = await _connectionRepo.GetAllAsync();
                    chosenConnection = all.FirstOrDefault(c => c.IsActive && c.Verified);
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Failed to get all connections for fallback selection");
                }
            }

            if (chosenConnection != null)
            {
                var connection = chosenConnection;
                payload["db_id"] = connection.Id.ToString();
                payload["server"] = connection.ServerName.Replace("\\\\", "\\");
                payload["database_name"] = connection.DatabaseName;
                payload["auth_mode"] = connection.AuthMode?.ToLower();
                payload["username"] = connection.Username;
                payload["password"] = connection.PasswordEncrypted;
                payload["trust_certificate"] = connection.TrustCertificate;
                payload["connection_timeout"] = connection.ConnectionTimeout;
            }
            else
            {
                _logger.LogDebug("No verified connection chosen to include in payload");
            }

            // Log payload JSON for debugging
            var payloadJson = JsonSerializer.Serialize(payload, _jsonOptions);
            _logger.LogDebug("POST /api/query payload: {PayloadJson}", payloadJson);

            var response = await _http.PostAsJsonAsync(
                "api/query",
                payload,
                _jsonOptions);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                _logger.LogError("AI provider /api/query returned {StatusCode}: {Body}", response.StatusCode, body);
                // Surface the provider body in the exception for quicker diagnosis (you can remove this later)
                throw new ApplicationException($"System was unable to respond to the request. Provider returned {(int)response.StatusCode}: {body}");
            }

            var result = await response.Content
                .ReadFromJsonAsync<JsonElement>(_jsonOptions);

            if (result.ValueKind == JsonValueKind.Undefined)
            {
                _logger.LogError("AI provider /api/query returned undefined JSON");
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetReplyAsync failed");
            // system failure → 500
            throw new ApplicationException("System was unable to respond to the request");
        }
    }
}