using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http.Json;
using AIChatbot.web.Models.Chat;
using AIChatbot.web.Models;

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
            if (res == null || !res.IsSuccessStatusCode)
                return new List<ChatSessionDto>();
            return await res.Content.ReadFromJsonAsync<List<ChatSessionDto>>() ?? new List<ChatSessionDto>();
        }

        public async Task<List<ChatMessageDto>> GetMessagesAsync(Guid sessionId)
        {
            var res = await _api.GetAsync($"/api/chat/V1/Conversation-engine/{sessionId}/messages");
            if (res == null || !res.IsSuccessStatusCode)
                return new List<ChatMessageDto>();
            return await res.Content.ReadFromJsonAsync<List<ChatMessageDto>>() ?? new List<ChatMessageDto>();
        }
        public async Task<ChatExecutionResult?> SendMessageAsync(SendMessageRequest req)
        {
            var res = await _api.PostAsync("/api/chat/V1/Conversation-engine/Push-Query/Session!", req);

            // TEMP DEBUG
            if (res == null)
                throw new Exception("ApiClient returned null — timeout or connection refused");

            if (!res.IsSuccessStatusCode)
                throw new Exception($"API returned {(int)res.StatusCode} {res.StatusCode} — {await res.Content.ReadAsStringAsync()}");

            return await res.Content.ReadFromJsonAsync<ChatExecutionResult>();
        }


        public async Task<bool> DeleteSessionAsync(Guid sessionId)
        {
            var res = await _api.DeleteAsync($"/api/chat/V1/Conversation-engine/{sessionId}/Delete");
            return res != null && res.IsSuccessStatusCode;
        }

        public async Task<ChatExecutionResult?> RetryAsync(Guid sessionId, Guid messageId)
        {
            var res = await _api.PostAsync(
                $"/api/chat/V1/Conversation-engine/{sessionId}/messages/{messageId}/retry", new { });
            if (res == null || !res.IsSuccessStatusCode)
                return null;
            return await res.Content.ReadFromJsonAsync<ChatExecutionResult>();
        }
    }
}