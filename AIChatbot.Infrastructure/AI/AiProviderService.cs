using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Common;
using AIChatbot.Domain.Entities;
using System.Net.Http.Json;

namespace AIChatbot.Infrastructure.AI;

/// <summary>
/// AI provider implementation using Flask LLM backend
/// </summary>
public class AiProviderService : IAiProviderService
{
    private readonly HttpClient _http;

    public AiProviderService(HttpClient http)
    {
        _http = http;
    }

    /// <summary>
    /// Sends query + last 5 messages history to LLM
    /// Receives structured JSON response
    /// </summary>
    public async Task<LlmResponse> GetReplyAsync(
        string userQuery,
        IEnumerable<Message> history)
    {
        var payload = new
        {
            query = userQuery,
            conversation_history = history.Select(m => new
            {
                role = m.Role == "assistant" ? "bot" : m.Role,
                message = m.Content
            }).ToList()
        };

        var response = await _http.PostAsJsonAsync(
       "api/query",
       payload);


        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<LlmResponse>();

        if (result == null)
            throw new Exception("LLM returned null response");

        return result;
    }
}
