using AIChatbot.Application.DTOs;
using AIChatbot.Application.Interfaces;
using System.Net.Http.Json;
using System.Text;

namespace AIChatbot.Infrastructure.AI;

public class AiProviderService : IAiProviderService
{
    private readonly HttpClient _http;

    public AiProviderService(HttpClient http)
    {
        _http = http;
    }

    public async Task<string> GetReplyAsync(IEnumerable<MessageDto> context)
    {
        var sb = new StringBuilder();

        foreach (var msg in context)
        {
            if (msg.Role == "user")
                sb.AppendLine($"User: {msg.Content}");
            else
                sb.AppendLine($"Assistant: {msg.Content}");
        }

        sb.AppendLine("Assistant:");

        var payload = new
        {
            model = "gemma3:1b",
            prompt = sb.ToString(),
            stream = false
        };

        try
        {
            var response = await _http.PostAsJsonAsync(
                "http://localhost:11434/api/generate",
                payload);

            if (!response.IsSuccessStatusCode)
                return "AI service is currently unavailable.";

            var result =
                await response.Content.ReadFromJsonAsync<OllamaResponse>();

            return result?.response ?? "No response from model.";
        }
        catch (Exception)
        {
            // Prevent ER500
            return "AI service failed to respond.";
        }
    }

    private sealed class OllamaResponse
    {
        public string response { get; set; } = string.Empty;
    }
}
