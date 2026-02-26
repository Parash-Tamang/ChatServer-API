using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Common;
using AIChatbot.Application.RoleAccess.Models;
using AIChatbot.Application.RoleAccess.Services;
using AIChatbot.Domain.Entities;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace AIChatbot.Infrastructure.AI;

/// <summary>
/// LOCAL LLM provider (Ollama) for testing without Flask + schema pipeline.
/// Ignores RoleAccessResult and only uses:
/// - user query
/// - last 5 chat messages
/// </summary>
public class ResponseProvider : IAiProviderService
{
    private readonly HttpClient _http;

    public ResponseProvider(HttpClient http)
    {
        _http = http;
    }

    public async Task<LlmResponse> GetReplyAsync(
        string userQuery,
        IEnumerable<Message> history,
        RoleAccessResult schema   // REQUIRED by interface (ignored here)
    )
    {
        // 🔵 Build combined prompt
        var sb = new StringBuilder();

        sb.AppendLine("You are an AI assistant.");
        sb.AppendLine("Answer ONLY the current user question.");
        sb.AppendLine("Use previous messages only if relevant.");
        sb.AppendLine("Be concise and helpful.");
        sb.AppendLine();

        // Current query
        sb.AppendLine("=== CURRENT QUESTION ===");
        sb.AppendLine(userQuery);
        sb.AppendLine();

        // Conversation history
        sb.AppendLine("=== LAST 5 MESSAGES ===");

        if (history != null && history.Any())
        {
            foreach (var msg in history)
            {
                sb.AppendLine($"{msg.Role}: {msg.Content}");
            }
        }
        else
        {
            sb.AppendLine("No previous messages.");
        }

        sb.AppendLine();
        sb.AppendLine("=== RESPONSE ===");

        var finalPrompt = sb.ToString();

        Console.WriteLine("\n====== OLLAMA INPUT ======");
        Console.WriteLine(finalPrompt);
        Console.WriteLine("==========================\n");

        // 🔵 OLLAMA CALL
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

        Console.WriteLine("\n====== RAW OLLAMA JSON ======");
        Console.WriteLine(raw);
        Console.WriteLine("=============================\n");

        // 🔵 parse Ollama output
        string reply = "No reply";

        try
        {
            using var doc = JsonDocument.Parse(raw);

            if (doc.RootElement.TryGetProperty("response", out var r))
                reply = r.GetString() ?? "Empty response";
            else
                reply = raw;
        }
        catch
        {
            reply = raw;
        }

        Console.WriteLine("\n====== FINAL REPLY ======");
        Console.WriteLine(reply);
        Console.WriteLine("=========================\n");

        // 🔵 return DUMMY structured payload (compatible with pipeline)
        return new LlmResponse
        {
            Success = true,
            Query = userQuery,
            Message = reply,

            // Dummy SQL info
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