using AIChatbot.web.Dto;
using AIChatbot.web.Interfaces;
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

        public async Task<bool> SaveConnectionAsync(ConnectionRequestDto dto)
        {
            var response = await _apiClient.PostAsync(
                "api/supersetup/V1/setup-engine/connection", dto);
            return response.IsSuccessStatusCode;
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











        public class ConnectionListResponse
        {
            public List<ConnectionRequestDto> Connections { get; set; } = new();
            public object? Functions { get; set; }      // ← ignore but don't break
            public object? GlobalPrompt { get; set; }   // ← ignore but don't break
        }




    }
}
