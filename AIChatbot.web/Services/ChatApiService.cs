using AIChatbot.web.Models.Chat;
using System.Text.Json;

namespace AIChatbot.web.Services
{
    public class ChatApiService
    {
        private readonly ApiClient _api;

        public ChatApiService(ApiClient api)
        {
            _api = api;
        }

        public async Task<List<ChatSessionDto>> GetSessionsAsync()
        {
            var res = await _api.GetAsync("/api/chat/V1/Conversation-engine/Get/ChatSessions");


            if (!res.IsSuccessStatusCode)
                return new List<ChatSessionDto>();

            var json = await res.Content.ReadAsStringAsync();

            var data = JsonSerializer.Deserialize<List<ChatSessionDto>>(json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            return data ?? new List<ChatSessionDto>();
        }



        public async Task<List<ChatMessageDto>> GetMessagesAsync(Guid id)
        {
            var res = await _api.GetAsync(
                

              $"/api/chat/V1/Conversation-engine/{id}/messages");

            if (!res.IsSuccessStatusCode)
                return new();

            return await res.Content.ReadFromJsonAsync<List<ChatMessageDto>>()
                   ?? new();
        }

        public async Task<ChatExecutionResult?> SendMessageAsync(SendMessageRequest req)
        {
            var res = await _api.PostAsync(
                "/api/chat/V1/Conversation-engine/Push-Query/Session!",
                req);

            if (!res.IsSuccessStatusCode)
                return null;

            return await res.Content.ReadFromJsonAsync<ChatExecutionResult>();
        }

        public async Task<bool> DeleteSessionAsync(Guid id)
        {
            var res = await _api.DeleteAsync(
                $"/api/chat/V1/Conversation-engine/{id}/Delete");

            return res.IsSuccessStatusCode;
        }

        public async Task<ChatExecutionResult?> RetryAsync(Guid sessionId, Guid messageId)
        {
            var res = await _api.PostAsync(
                $"/api/chat/V1/Conversation-engine/{sessionId}/messages/{messageId}/retry",
                new { });

            if (!res.IsSuccessStatusCode)
                return null;

            return await res.Content.ReadFromJsonAsync<ChatExecutionResult>();
        }
    }
}
