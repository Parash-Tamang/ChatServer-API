using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Common;
using AIChatbot.Application.RoleAccess.Models;
using AIChatbot.Domain.Entities;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace AIChatbot.Infrastructure.AI;

public class ResponseProvider : IAiProviderService
{
    private readonly HttpClient _http;

    public ResponseProvider(HttpClient http)
    {
        _http = http;
    }

    // ----------------------------------------------------
    // Dummy DB verification (for Supersetup testing)
    // ----------------------------------------------------
    public Task<DatabaseSetupResult> PrepareDatabaseAsync(
        ConnectionString connection)
    {
        return Task.FromResult(new DatabaseSetupResult
        {
            Id = connection.Id,
            DbStatus = true
        });
    }

    // ----------------------------------------------------
    // LLM Reply using Ollama (Test Mode)
    // ----------------------------------------------------
    public async Task<LlmResponse> GetReplyAsync(
        string userQuery,
        IEnumerable<Message> history,
        RoleAccessResult schema)
    {
        var lastMessages = history?
            .TakeLast(5)
            .Select(m => $"{m.Role}: {m.Content}")
            .ToList() ?? new List<string>();

        var sb = new StringBuilder();

        sb.AppendLine("You are an AI assistant.");
        sb.AppendLine("Answer the current question.");
        sb.AppendLine();

        sb.AppendLine("Conversation History:");
        foreach (var msg in lastMessages)
        {
            sb.AppendLine(msg);
        }

        sb.AppendLine();
        sb.AppendLine("User Question:");
        sb.AppendLine(userQuery);
        sb.AppendLine();
        sb.AppendLine("Answer:");

        var finalPrompt = sb.ToString();

        var payload = new
        {
            model = "gemma3:1b",
            prompt = finalPrompt,
            stream = false
        };

        var response = await _http.PostAsJsonAsync(
            "http://localhost:11434/api/generate",
            payload);

        response.EnsureSuccessStatusCode();

        var raw = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(raw);

        var reply = doc.RootElement
            .GetProperty("response")
            .GetString();

        return new LlmResponse
        {
            Success = true,
            Query = userQuery,
            Message = reply ?? "[TEST MODE] No reply",
            SqlGenerated = null,
            Columns = Array.Empty<object>(),
            Rows = Array.Empty<object>(),
            RowCount = 0,
            WasReconstructed = false,
            ClarificationNeeded = false,
            TokenUsage = new
            {
                prompt = 0,
                completion = 0
            }
        };
    }
}