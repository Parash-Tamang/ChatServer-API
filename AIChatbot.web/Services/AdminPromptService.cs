using AIChatbot.web.Interfaces;
using AIChatbot.web.Models.Admin;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AIChatbot.web.Services
{
    public class AdminPromptService : IAdminPromptService
    {
        private readonly HttpClient _httpClient;
        private readonly ITokenService _tokenService;
        private readonly string _baseUrl;

        public AdminPromptService(IHttpClientFactory httpClientFactory, ITokenService tokenService, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient();
            _tokenService = tokenService;
            _baseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://192.168.40.71:5197";


            if (string.IsNullOrEmpty(_baseUrl))
                throw new InvalidOperationException("ApiSettings:BaseUrl is missing from configuration.");
        }
            
        private void AttachToken()
        {
            var token = _tokenService.GetAccessToken();
            if (!string.IsNullOrEmpty(token))
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
        }

        private StringContent ToJson(object obj) =>
            new StringContent(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json");

        public async Task<List<GlobalPromptDto>> GetGlobalFunctionsAsync()
        {
            AttachToken();
            var response = await _httpClient.GetAsync(
                $"{_baseUrl}/api/supersetup/V1/setup-engine/functions/global/Global_function");

            if (!response.IsSuccessStatusCode)
                return new List<GlobalPromptDto>();

            return await response.Content.ReadFromJsonAsync<List<GlobalPromptDto>>()
                   ?? new List<GlobalPromptDto>();
        }

        public async Task<GlobalPromptDto?> GetFunctionByIdAsync(string functionId)
        {
            AttachToken();
            var response = await _httpClient.GetAsync(
                $"{_baseUrl}/api/supersetup/V1/setup-engine/functions/{functionId}");

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<GlobalPromptDto>();
        }

        public async Task<bool> SaveGlobalPromptAsync(SaveGlobalPromptRequest request)
        {
            AttachToken();
            var response = await _httpClient.PostAsync(
                $"{_baseUrl}/api/supersetup/V1/setup-engine/Function/global",
                ToJson(request));

            return response.IsSuccessStatusCode;
        }

        public async Task<List<GlobalPromptDto>> GetConnectionFunctionsAsync(string connectionId)
        {
            AttachToken();
            var response = await _httpClient.GetAsync(
                $"{_baseUrl}/api/supersetup/V1/setup-engine/connections/{connectionId}/functions");

            // Temporary logging
            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"ConnectionId: {connectionId}");
            Console.WriteLine($"Status: {response.StatusCode}");
            Console.WriteLine($"Response: {content}");

            if (!response.IsSuccessStatusCode)
                return new List<GlobalPromptDto>();

            return await response.Content.ReadFromJsonAsync<List<GlobalPromptDto>>()
                   ?? new List<GlobalPromptDto>();
        }

        public async Task<bool> SaveLocalPromptAsync(SaveLocalPromptRequest request)
        {
            try
            {
                AttachToken();
                var response = await _httpClient.PostAsync(
                    $"{_baseUrl}/api/supersetup/V1/setup-engine/function/Local",
                    ToJson(request));

                var json = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"SaveLocal status: {(int)response.StatusCode}");
                Console.WriteLine($"SaveLocal response: {json}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SaveLocal EXCEPTION: {ex.Message}");
                Console.WriteLine($"SaveLocal STACK: {ex.StackTrace}");
                return false;
            }
        }
    }
}