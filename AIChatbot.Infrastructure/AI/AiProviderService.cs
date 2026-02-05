using AIChatbot.Application.Abstractions;
using AIChatbot.Domain.Entities;
using System.Net.Http.Json;
using System.Text;

namespace AIChatbot.Infrastructure.AI;


/// AI provider implementation using Ollama local API

public class AiProviderService : IAiProviderService
{
    private readonly HttpClient _http;

    public AiProviderService(HttpClient http)
    {
        _http = http;
    }

   
    /// Generates an assistant reply based on conversation context
   
    public async Task<string> GetReplyAsync(IEnumerable<Message> context)
    {
        // Build prompt from conversation history
        var sb = new StringBuilder();

        foreach (var msg in context)
        {
            sb.AppendLine(
                msg.Role == "user"
                    ? $"User: {msg.Content}"
                    : $"Assistant: {msg.Content}");
        }

        // Instruct the model to continue as assistant
        sb.AppendLine("Assistant:");

        var payload = new
        {
            model = "gemma3:1b",
            prompt = sb.ToString(),
            stream = false
        };

        // Send request to local Ollama instance
        var response = await _http.PostAsJsonAsync(
            "http://localhost:11434/api/generate",
            payload);

        // Fail fast on non-success responses
        response.EnsureSuccessStatusCode();

        var result =
            await response.Content.ReadFromJsonAsync<OllamaResponse>();

        if (string.IsNullOrWhiteSpace(result?.Response))
            throw new InvalidOperationException("Empty AI response");

        return result.Response;
    }

    
    /// Minimal response contract from Ollama API
   
    private sealed class OllamaResponse
    {
        public string Response { get; set; } = string.Empty;
    }
}
