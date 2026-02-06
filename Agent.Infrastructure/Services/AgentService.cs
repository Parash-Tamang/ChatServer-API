using Agent.Application.Dto.UserManagement;
using Agent.Application.Interface;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace Agent.Infrastructure.Services
{
    public class AgentService : IAgentService 
    {
        private readonly HttpClient _client;
        private readonly string _baseUrl;

        public AgentService (HttpClient client, IConfiguration config)
        {
            _client = client;
            _baseUrl = config.GetValue<string>("FlaskApi:BaseUrl") ?? "http://localhost:5000";
        }
        public Task<UserMessageResponseDto> SendDataAgentAsync(UserMessageRequestDto userMessageRequestDto)
        {
            throw new NotImplementedException();

        }

        public Task<SQLQueryResponseDto> ExecuteSQlQueryAsync(SQLQueryReceivedDto sqlQueryDto)
        {
            throw new NotImplementedException();
        }


        public async Task<AgentResponseDto?> GenerateAsync(
     string query,
     List<UserMessageResponseDto> lastMessages)
        {
            var history = lastMessages.Select(m => new
            {
                role = m.SenderType,
                message = m.MessageText
            });

            var payload = new
            {
                query = query,
                conversation_history = history
            };

            var json = JsonSerializer.Serialize(payload);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync($"{_baseUrl}/api/query", content);

            response.EnsureSuccessStatusCode();

            if (response == null)
            {
                return null;
            }

            var data = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<AgentResponseDto?>(data);
        }
    }
}

