using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Common;
using AIChatbot.Application.RoleAccess.Models;
using AIChatbot.Domain.Entities;
using System.Net.Http.Json;
using System.Text.Json;

namespace AIChatbot.Infrastructure.AI;

/// <summary>
/// AI provider implementation using Flask LLM backend.
/// Responsible ONLY for transporting structured data to Python.
/// Does NOT contain business logic.
/// </summary>
public class AiProviderService : IAiProviderService
{
    private readonly HttpClient _http;

    public AiProviderService(HttpClient http)
    {
        _http = http;
    }

    public async Task<LlmResponse> GetReplyAsync(
        string userQuery,
        IEnumerable<Message> history,
        RoleAccessResult schema)
    {
        var payload = new
        {
            query = userQuery,

            conversation_history = history.Select(m => new
            {
                role = m.Role == "assistant" ? "bot" : m.Role,
                message = m.Content
            }),

            role_access = schema   // 🔥 send filtered schema to Python
        };

        var response = await _http.PostAsJsonAsync("api/query", payload);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<LlmResponse>();

        if (result == null)
            throw new Exception("LLM returned null");

        return result;
    }
}