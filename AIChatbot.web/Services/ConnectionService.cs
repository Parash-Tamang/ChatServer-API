using AIChatbot.web.Dto;
using AIChatbot.web.Interfaces;
using AIChatbot.web.Models.Admin;
using AIChatbot.web.Models.Chat;
using System.Text.Json;

namespace AIChatbot.web.Services
{
    public class ConnectionService : IConnectionService
    {
        private readonly ApiClient _apiClient;

        public ConnectionService(ApiClient apiClient)
        {
            _apiClient = apiClient; // ← reuses your base url + token automatically
        }



        //// ConnectionService.cs
        //public async Task<ServiceResult> SaveConnectionAsync(ConnectionRequestDto dto)
        //{
        //    var response = await _apiClient.PostAsync(
        //        "api/supersetup/V1/setup-engine/connection/test", dto);

        //    var json = await response?.Content.ReadAsStringAsync();
        //    var result = JsonSerializer.Deserialize<ServiceResult>(json,
        //        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //    return result ?? new ServiceResult { Success = false, Message = "No response from API." };
        //}
        public async Task<ServiceResult> SaveConnectionAsync(ConnectionRequestDto dto)
        {
            var response = await _apiClient.PostAsync(
                "api/supersetup/V1/setup-engine/connection/test", dto);

            var json = await response?.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ServiceResult>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return result ?? new ServiceResult { Success = false, Message = "No response from API." };
        }

        public async Task<List<ConnectionRequestDto>> GetAllConnectionsAsync()
        {
            var response = await _apiClient.GetAsync("api/supersetup/V1/setup-engine/connections");
            if (response == null || !response.IsSuccessStatusCode)
                return new List<ConnectionRequestDto>();

            var json = await response.Content.ReadAsStringAsync();
            var list = System.Text.Json.JsonSerializer.Deserialize<List<ConnectionRequestDto>>(json,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var connections = list ?? new List<ConnectionRequestDto>();
            connections.ForEach(c => c.Password = null);
            return connections;
        }
        public async Task<bool> UpdateConnectionAsync(ConnectionRequestDto dto)
        {
            var response = await _apiClient.PostAsync(
                "api/supersetup/V1/setup-engine/connection/test", dto);

            var json = await response?.Content.ReadAsStringAsync();
            Console.WriteLine($"Update response: {json}");
                
            return response?.IsSuccessStatusCode ?? false;
        }
        public async Task<bool> SetActiveConnectionAsync(Guid id, List<ConnectionRequestDto> allConnections)
        {
            foreach (var conn in allConnections)
            {
                conn.IsActive = conn.Id == id;
                var response = await _apiClient.PostAsync("api/supersetup/V1/setup-engine/connection", conn);
                if (response == null || !response.IsSuccessStatusCode)
                    return false;
            }
            return true;
        }
        public async Task<ManagePromptsViewModel> GetManagePromptsAsync()
        {
            var connectionsResponse = await _apiClient.GetAsync("api/supersetup/V1/setup-engine/connections");
            var globalPrompt = await GetGlobalPromptAsync();

            var connections = new List<ConnectionRequestDto>();
            if (connectionsResponse != null && connectionsResponse.IsSuccessStatusCode)
            {
                var json = await connectionsResponse.Content.ReadAsStringAsync();
                connections = System.Text.Json.JsonSerializer.Deserialize<List<ConnectionRequestDto>>(json,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            }

            return new ManagePromptsViewModel
            {
                Connections = connections,
                GlobalPrompts = globalPrompt?.Id != null ? new List<GlobalPromptDto> { globalPrompt } : new(),
                LocalPrompts = new Dictionary<string, List<LocalPromptDto>>()
            };
        }

        public async Task<bool> ActivateConnectionAsync(Guid connectionId)
        {
            var payload = new { connectionId };
            var response = await _apiClient.PostAsync(
                "api/supersetup/V1/setup-engine/connection/activate", payload);
            return response?.IsSuccessStatusCode ?? false;
        }

        public async Task<bool> UpdateKnowledgeBaseAsync(Guid connectionId)
        {
            var payload = new { connectionId };
            var response = await _apiClient.PostAsync(
                "api/supersetup/V1/setup-engine/connection/update-kb", payload);
            return response?.IsSuccessStatusCode ?? false;
        }
        // Update ConnectionListResponse
        public class ConnectionListResponse
        {
            public List<ConnectionRequestDto> Connections { get; set; } = new();
            public List<GlobalPromptDto> GlobalPrompt { get; set; } = new();
            public Dictionary<string, List<LocalPromptDto>> Functions { get; set; } = new();
        }

        public async Task<GlobalPromptDto> GetGlobalPromptAsync()
        {
            var response = await _apiClient.GetAsync("api/supersetup/V1/setup-engine/functions/global");
            if (response == null || !response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            var list = System.Text.Json.JsonSerializer.Deserialize<List<GlobalPromptDto>>(json,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return list?.FirstOrDefault();
        }

        public async Task<bool> SaveGlobalPromptAsync(GlobalPromptDto dto)
        {
            var response = await _apiClient.PostAsync("api/supersetup/V1/setup-engine/global", dto);
            return response?.IsSuccessStatusCode ?? false;
        }

        public async Task<LocalPromptDto> GetConnectionFunctionAsync(string connectionId)
        {
            var response = await _apiClient.GetAsync($"api/supersetup/V1/setup-engine/connections/{connectionId}/functions");
            if (response == null || !response.IsSuccessStatusCode) return null;
            var json = await response.Content.ReadAsStringAsync();
            return System.Text.Json.JsonSerializer.Deserialize<LocalPromptDto>(json,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<LocalPromptDto> GetLocalPromptAsync(string functionId)
        {
            var response = await _apiClient.GetAsync($"api/supersetup/V1/setup-engine/functions/{functionId}");
            if (response == null || !response.IsSuccessStatusCode) return null;
            var json = await response.Content.ReadAsStringAsync();
            return System.Text.Json.JsonSerializer.Deserialize<LocalPromptDto>(json,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<bool> SaveLocalPromptAsync(LocalPromptDto dto)
        {
            var response = await _apiClient.PostAsync("api/supersetup/V1/setup-engine/global", dto);
            return response?.IsSuccessStatusCode ?? false;
        }

        public async Task<bool> SetPromptingModeAsync(string connectionId, int promptingMode)
        {
            var payload = new { connectionId, promptingMode };
            var response = await _apiClient.PostAsync("api/supersetup/V1/setup-engine/connection/prompt-mode", payload);
            return response?.IsSuccessStatusCode ?? false;
        }
        public async Task<bool> DeleteConnectionAsync(Guid connectionId)
        {
            var response = await _apiClient.DeleteAsync(
                $"api/supersetup/V1/setup-engine?connectionId={connectionId}");
            return response?.IsSuccessStatusCode ?? false;
        }

        public async Task<bool> DeleteFunctionAsync(Guid connectionId, string functionId)
        {
            var response = await _apiClient.DeleteAsync(
                $"api/supersetup/V1/setup-engine?connectionId={connectionId}&functionId={functionId}");
            return response?.IsSuccessStatusCode ?? false;
        }

    }
}
