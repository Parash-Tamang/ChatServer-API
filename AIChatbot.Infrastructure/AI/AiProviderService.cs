using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Common;
using AIChatbot.Application.RoleAccess.Models;
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
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly IConnectionRepository _connectionRepo;
    private readonly IPromptFunctionRepository _promptRepo;
    private readonly ILogger<AiProviderService> _logger;

    public AiProviderService(
        HttpClient http,
        IConnectionRepository connectionRepo,
        IPromptFunctionRepository promptRepo,
        ILogger<AiProviderService> logger)
    {
        _http = http;
        _connectionRepo = connectionRepo;
        _promptRepo = promptRepo;
        _logger = logger;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    // ============================================================
    // ✅ DATABASE SETUP
    // ============================================================
    public async Task<DatabaseSetupResult> PrepareDatabaseAsync(ConnectionString connection)
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
            };

            var response = await _http.PostAsJsonAsync(
                "api/knowledgebase/setup",
                payload,
                _jsonOptions);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.UnsupportedMediaType)
                {
                    throw new BadHttpRequestException(
                        "Unsupported media type from downstream service",
                        StatusCodes.Status415UnsupportedMediaType);
                }

                throw new ApplicationException("Knowledgebase setup failed. Please verify connection.");
            }

            var result = await response.Content
                .ReadFromJsonAsync<JsonElement>(_jsonOptions);

            if (result.ValueKind == JsonValueKind.Undefined)
                throw new ApplicationException("Invalid response from knowledgebase service.");

            var success = result.TryGetProperty("success", out var s) && s.GetBoolean();

            return new DatabaseSetupResult
            {
                Id = connection.Id,
                DbStatus = success
            };
        }
        catch (BadHttpRequestException) { throw; }
        catch (TimeoutException) { throw; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PrepareDatabaseAsync failed");
            throw new ApplicationException("System was unable to prepare the database.");
        }
    }

    // ============================================================
    // ✅ CHAT / QUERY EXECUTION
    // ============================================================
    public async Task<LlmResponse> GetReplyAsync(
        string userQuery,
        IEnumerable<Message> history,
        RoleAccessResult schema)
    {
        try
        {
            // =============================
            // 🔒 VALIDATION
            // =============================
            if (string.IsNullOrWhiteSpace(userQuery))
                throw new BadHttpRequestException("Query cannot be empty.");

            var payload = new Dictionary<string, object?>
            {
                ["query"] = userQuery,
                ["conversation_history"] = history?
                    .TakeLast(10)
                    .Select(m => new { message = m.Content })
                    .ToList(),
                ["save_results"] = true
            };

            // =============================
            // 🔐 SELECT CONNECTION
            // =============================
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
                var all = await _connectionRepo.GetAllAsync();
                chosenConnection = all.FirstOrDefault(c => c.IsActive && c.Verified);
            }

            if (chosenConnection == null)
                throw new KeyNotFoundException("No active database connection found.");

            var connection = chosenConnection;

            // ============================================================
            // 🔥 PROMPT MODE LOGIC
            // ============================================================
            List<PromptFunction> selectedFunctions;

            if (connection.PromptingMode == 1)
            {
                selectedFunctions = await _promptRepo.GetByConnectionIdAsync(connection.Id);

                if (!selectedFunctions.Any())
                    throw new KeyNotFoundException("No local functions configured.");
            }
            else if (connection.PromptingMode == 0)
            {
                selectedFunctions = await _promptRepo.GetGlobalFunctionsAsync();

                if (!selectedFunctions.Any())
                    throw new KeyNotFoundException("No global functions configured.");
            }
            else
            {
                throw new BadHttpRequestException("Invalid prompting mode configuration.");
            }

            // ============================================================
            // 🔥 PREPARE FUNCTION PAYLOAD (FIXED)
            // ============================================================
            var functionPayload = selectedFunctions.Select(f => new
            {
                functionName = f.FunctionName,
                systemPrompt = f.SystemPrompt
            }).ToList();

            // ============================================================
            // ⚠️ SAFE MODE (DO NOT SEND YET)
            // ============================================================
            // When Python is ready, UNCOMMENT:
            //
            // payload["functions"] = functionPayload;
            //
            // ============================================================

            // =============================
            // 🔗 CONNECTION DETAILS
            // =============================
            payload["db_id"] = connection.Id.ToString();
            payload["server"] = connection.ServerName.Replace("\\\\", "\\");
            payload["database_name"] = connection.DatabaseName;
            payload["auth_mode"] = connection.AuthMode?.ToLower();
            payload["username"] = connection.Username;
            payload["password"] = connection.PasswordEncrypted;
            payload["trust_certificate"] = connection.TrustCertificate;
            payload["connection_timeout"] = connection.ConnectionTimeout;

            // =============================
            // 🚀 CALL PYTHON
            // =============================
            var response = await _http.PostAsJsonAsync(
                "api/query",
                payload,
                _jsonOptions);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.UnsupportedMediaType)
                {
                    throw new BadHttpRequestException(
                        "Unsupported media type from AI provider",
                        StatusCodes.Status415UnsupportedMediaType);
                }

                throw new ApplicationException("AI service failed to process the request.");
            }

            var result = await response.Content
                .ReadFromJsonAsync<JsonElement>(_jsonOptions);

            if (!result.TryGetProperty("success", out var s) || !s.GetBoolean())
                throw new TimeoutException("The model was unable to respond.");

            // =============================
            // ✅ RETURN RESPONSE
            // =============================
            return new LlmResponse
            {
                Success = true,
                Message = result.TryGetProperty("message", out var m)
                    ? m.GetString()
                    : "",
                SqlGenerated = result.TryGetProperty("sql_generated", out var sql)
                    ? sql.GetString()
                    : null,
                RowCount = result.TryGetProperty("row_count", out var rc)
                    ? rc.GetInt32()
                    : 0,
                Columns = result.TryGetProperty("columns", out var cols)
                    ? cols.Deserialize<object[]>(_jsonOptions) ?? Array.Empty<object>()
                    : Array.Empty<object>(),
                Rows = result.TryGetProperty("rows", out var rows)
                    ? rows.Deserialize<object[]>(_jsonOptions) ?? Array.Empty<object>()
                    : Array.Empty<object>(),
                ExcelGenerated = result.TryGetProperty("excel_generated", out var eg) && eg.GetBoolean(),
                ExcelAvailableNow = result.TryGetProperty("excel_available_now", out var ean)
                    ? ean.Deserialize<ExcelAvailableNow>(_jsonOptions)
                    : null,
                GraphType = result.TryGetProperty("graph_type", out var gt)
                    ? gt.GetString()
                    : null,
                GraphTitle = result.TryGetProperty("graph_title", out var gtitle)
                    ? gtitle.GetString()
                    : null,
                GraphImageUrl = result.TryGetProperty("graph_image_url", out var giu)
                    ? giu.GetString()
                    : null,
                GraphImageBase64 = result.TryGetProperty("graph_image_base64", out var gib)
                    ? gib.GetString()
                    : null
            };
        }
        catch (BadHttpRequestException) { throw; }
        catch (KeyNotFoundException) { throw; }
        catch (TimeoutException) { throw; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetReplyAsync failed");
            throw new ApplicationException("System was unable to respond to the request.");
        }
    }
}