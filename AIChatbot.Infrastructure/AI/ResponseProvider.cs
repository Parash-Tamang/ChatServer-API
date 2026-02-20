using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Common;
using AIChatbot.Domain.Entities;
using System.Text;
using System.Text.Json;
using System.Net.Http.Json;

namespace AIChatbot.Infrastructure.AI;

/// <summary>
/// TEMPORARY provider to test Ollama directly (without Flask)
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
        IEnumerable<Message> history)
    {
        // 🔵 Build combined prompt
        var sb = new StringBuilder();

        // System instruction
        sb.AppendLine("You are an AI assistant. Your task is to answer the user's current question accurately and relevantly.");
        sb.AppendLine("Follow these rules strictly:");
        sb.AppendLine("1. Focus primarily on the CURRENT QUESTION.");
        sb.AppendLine("2. Use the provided last 5 messages only as context memory if they are relevant.");
        sb.AppendLine("3. If history is null or irrelevant, ignore it.");
        sb.AppendLine("4. Do NOT assume information beyond what is provided.");
        sb.AppendLine("5. Provide a clear, precise, and helpful response.");
        sb.AppendLine();

        // Current query
        sb.AppendLine("=== CURRENT QUESTION ===");
        sb.AppendLine(userQuery);
        sb.AppendLine();

        // Conversation memory
        sb.AppendLine("=== LAST 5 MESSAGES (CONTEXT MEMORY - MAY BE NULL) ===");

        if (history != null && history.Any())
        {
            foreach (var msg in history)
            {
                sb.AppendLine($"{msg.Role}: {msg.Content}");
            }
        }
        else
        {
            sb.AppendLine("No previous messages available.");
        }

        sb.AppendLine();

        // Final instruction
        sb.AppendLine("=== RESPONSE INSTRUCTION ===");
        sb.AppendLine("Generate the best possible answer to the CURRENT QUESTION using the context above only when necessary.");
        sb.AppendLine("Keep the response relevant, structured, and directly useful.");


        var finalPrompt = sb.ToString();

        Console.WriteLine("\n====== OLLAMA INPUT ======");
        Console.WriteLine(finalPrompt);
        Console.WriteLine("==========================\n");

        // 🔵 call ollama
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

        // 🔴 READ RAW RESPONSE FIRST
        var raw = await response.Content.ReadAsStringAsync();

        Console.WriteLine("\n====== RAW OLLAMA JSON ======");
        Console.WriteLine(raw);
        Console.WriteLine("=============================\n");

        // 🔵 Parse safely
        string reply = "No reply";

        try
        {
            using var doc = JsonDocument.Parse(raw);

            if (doc.RootElement.TryGetProperty("response", out var r))
                reply = r.GetString() ?? "Empty response";
            else
                reply = raw; // fallback
        }
        catch
        {
            reply = raw;
        }

        Console.WriteLine("\n====== FINAL REPLY ======");
        Console.WriteLine(reply);
        Console.WriteLine("=========================\n");

        // 🔵 Return structured JSON like Flask
        return new LlmResponse
        {
            Success = true,
            Query = userQuery,
            Message = reply,
            SqlGenerated = "demo sql",
            Columns = new object[] { "col1", "col2" },
            Rows = new object[] { },
            RowCount = 0,
            WasReconstructed = false,
            ClarificationNeeded = false,
            TokenUsage = new { prompt = 0, completion = 0 }
        };
    }
}
