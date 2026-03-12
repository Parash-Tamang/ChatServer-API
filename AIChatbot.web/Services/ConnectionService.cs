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

        public async Task<bool> ConnectAsync(SqlConnectionViewModel model)
        {
            // Map ViewModel → DTO
            var dto = new ConnectionRequestDto
            {
                Id = Guid.NewGuid(),
                ServerName = model.Server,
                DatabaseName = model.Database,
                AuthMode = model.AuthMode,
                Username = model.Username,
                Password = model.Password,
                TrustCertificate = model.EncryptConnection,
                ConnectionTimeout = model.Timeout,
                IsActive = true
            };

            var response = await _apiClient.PostAsync(
                "api/supersetup/V1/setup-engine/connection", dto);

            return response.IsSuccessStatusCode;
        }
    }
}