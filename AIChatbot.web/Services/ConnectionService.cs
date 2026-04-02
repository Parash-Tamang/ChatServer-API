using AIChatbot.web.Dto;
using AIChatbot.web.Interfaces;
using AIChatbot.web.Models.Admin;
using AIChatbot.web.Models.Chat;

namespace AIChatbot.web.Services
{
    public class ConnectionService : IConnectionService
    {
        private readonly ApiClient _apiClient;

        public ConnectionService(ApiClient apiClient)
        {
            _apiClient = apiClient; // ← reuses your base url + token automatically
        }

        public async Task<ServiceResult> SaveConnectionAsync(ConnectionRequestDto dto)
        {
            var response = await _apiClient.PostAsync(
                "api/supersetup/V1/setup-engine/connection/test", dto);

            if (response == null)
                return new ServiceResult { Success = false, Message = "Server unreachable. Please try again." };

            var json = await response.Content.ReadAsStringAsync();

            var result = System.Text.Json.JsonSerializer.Deserialize<ServiceResult>(json,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return result ?? new ServiceResult { Success = false, Message = "Unexpected error occurred." };
        }
        public async Task<List<ConnectionRequestDto>> GetAllConnectionsAsync()
        {
            var response = await _apiClient.GetAsync("api/supersetup/V1/setup-engine/all");
            if (response == null || !response.IsSuccessStatusCode)
                return new List<ConnectionRequestDto>();

            var json = await response.Content.ReadAsStringAsync();
            var result = System.Text.Json.JsonSerializer.Deserialize<ConnectionListResponse>(json, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result?.Connections ?? new List<ConnectionRequestDto>();
        }

        public async Task<bool> UpdateConnectionAsync(ConnectionRequestDto dto)
        {
            var response = await _apiClient.PostAsync("api/supersetup/V1/setup-engine/connection", dto);
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
            var response = await _apiClient.GetAsync("api/supersetup/V1/setup-engine/all");
            if (response == null || !response.IsSuccessStatusCode)
                return new ManagePromptsViewModel();

            var json = await response.Content.ReadAsStringAsync();
            var result = System.Text.Json.JsonSerializer.Deserialize<ConnectionListResponse>(json,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result == null) return new ManagePromptsViewModel();

            return new ManagePromptsViewModel
            {
                Connections = result.Connections ?? new(),
                GlobalPrompts = result.GlobalPrompt ?? new(),
                LocalPrompts = result.Functions ?? new()
            };
        }

        // Update ConnectionListResponse
        public class ConnectionListResponse
        {
            public List<ConnectionRequestDto> Connections { get; set; } = new();
            public List<GlobalPromptDto> GlobalPrompt { get; set; } = new();
            public Dictionary<string, List<LocalPromptDto>> Functions { get; set; } = new();
        }



    }
}
