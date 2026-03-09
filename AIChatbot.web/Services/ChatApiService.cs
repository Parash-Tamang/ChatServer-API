using System;

using System.Collections.Generic;

using System.Threading.Tasks;

using System.Net.Http.Json;



using AIChatbot.web.Models.Chat;

using AIChatbot.web;

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



        // ? GET /api/chat/V1/Conversation-engine/Get/ChatSessions

        public async Task<List<ChatSessionDto>> GetSessionsAsync()

        {

            var res = await _api.GetAsync("/api/chat/V1/Conversation-engine/Get/ChatSessions");



            if (!res.IsSuccessStatusCode)

                return new List<ChatSessionDto>();



            return await res.Content.ReadFromJsonAsync<List<ChatSessionDto>>() ?? new List<ChatSessionDto>();

        }



        // ? GET /api/chat/V1/Conversation-engine/{sessionId}/messages

        public async Task<ChatSessionDto?> GetSessionAsync(Guid sessionId)

        {

            var res = await _api.GetAsync($"/api/chat/V1/Conversation-engine/{sessionId}");



            if (!res.IsSuccessStatusCode)

                return null;



            return await res.Content.ReadFromJsonAsync<ChatSessionDto>();

        }



        // ? POST /api/chat/V1/Conversation-engine/Push-Query/Session!

        public async Task<ChatSessionDto?> CreateSessionAsync()

        {

            var res = await _api.PostAsync("/api/chat/V1/Conversation-engine/Push-Query/Session!", new { });



            if (!res.IsSuccessStatusCode)

                return null;



            return await res.Content.ReadFromJsonAsync<ChatSessionDto>();

        }



        // ? DELETE /api/chat/V1/Conversation-engine/{sessionId}/Delete

        public async Task<bool> DeleteSessionAsync(Guid sessionId)

        {

            var res = await _api.DeleteAsync($"/api/chat/V1/Conversation-engine/{sessionId}/Delete");

            return res.IsSuccessStatusCode;

        }



        // ? POST /api/chat/V1/Conversation-engine/Push-Query/Session!

        public async Task<ChatExecutionResult?> SendMessageAsync(SendMessageRequest req)

        {

            var res = await _api.PostAsync("/api/chat/V1/Conversation-engine/Push-Query/Session!", req);



            if (!res.IsSuccessStatusCode)

                return null;



            return await res.Content.ReadFromJsonAsync<ChatExecutionResult>();

        }



        // ? GET /api/chat/V1/Conversation-engine/{sessionId}/messages

        public async Task<List<ChatMessageDto>> GetMessagesAsync(Guid sessionId)

        {

            var res = await _api.GetAsync($"/api/chat/V1/Conversation-engine/{sessionId}/messages");



            if (!res.IsSuccessStatusCode)

                return new List<ChatMessageDto>();



            return await res.Content.ReadFromJsonAsync<List<ChatMessageDto>>() ?? new List<ChatMessageDto>();

        }



        // ? POST /api/chat/V1/Conversation-engine/{sessionId}/messages/{messageId}/retry

        public async Task<ChatExecutionResult?> RetryAsync(Guid sessionId, Guid messageId)

        {

            var res = await _api.PostAsync($"/api/chat/V1/Conversation-engine/{sessionId}/messages/{messageId}/retry", new { });



            if (!res.IsSuccessStatusCode)

                return null;



            return await res.Content.ReadFromJsonAsync<ChatExecutionResult>();

        }

    }

}

