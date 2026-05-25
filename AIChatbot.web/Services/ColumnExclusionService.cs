using AIChatbot.web.Dto;
using AIChatbot.web.Interfaces;
using System.Text.Json;

namespace AIChatbot.web.Services
{
    public class ColumnExclusionService : IColumnExclusionService
    {
        private readonly ApiClient _apiClient;
        private readonly ILogger<ColumnExclusionService> _logger;

        public ColumnExclusionService(ApiClient apiClient, ILogger<ColumnExclusionService> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        public async Task<List<SchemaItemDto>> GetSchemaAsync(Guid connectionId)
        {
            try
            {
                var response = await _apiClient.GetAsync(
                    $"/api/access/V1/Data-Setup-engine/GETschema/{connectionId}");

                if (response is { IsSuccessStatusCode: true })
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<List<SchemaItemDto>>(json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return result ?? new();
                }

                _logger.LogWarning("GetSchema failed. Status: {Status}", response?.StatusCode);
                return new();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching schema for connection {ConnectionId}", connectionId);
                return new();
            }
        }

        public async Task<List<string>> GetExclusionsAsync(Guid connectionId)
        {
            try
            {
                var response = await _apiClient.GetAsync(
                    $"/api/access/V1/Data-Setup-engine/Exclude-from-DB/{connectionId}");

                if (response is { IsSuccessStatusCode: true })
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<List<string>>(json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return result ?? new();
                }

                _logger.LogWarning("GetExclusions failed. Status: {Status}", response?.StatusCode);
                return new();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching exclusions for connection {ConnectionId}", connectionId);
                return new();
            }
        }

        public async Task<bool> SaveExclusionsAsync(ExclusionSaveDto dto)
        {
            try
            {
                var response = await _apiClient.PostAsync(
                    "/api/access/V1/Data-Setup-engine/Exclude-from-DB/save", dto);
                return response is { IsSuccessStatusCode: true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving exclusions for connection {ConnectionId}", dto.ConnectionId);
                return false;
            }
        }
    }
}