using AIChatbot.web.Interfaces;
namespace AIChatbot.web.Services
{
    public class ApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;

        public ApiClient(HttpClient httpClient, IConfiguration configuration, ITokenService tokenService)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _tokenService = tokenService;

            // Set base address from configuration. No fallback � require the configuration key.
            var apiBaseUrl = _configuration["ApiSettings:BaseUrl"];
            if (string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                throw new InvalidOperationException("Configuration key 'ApiSettings:BaseUrl' is missing. Please set ApiSettings:BaseUrl in appsettings.json.");
            }
            _httpClient.BaseAddress = new Uri(apiBaseUrl);
            _httpClient.Timeout = Timeout.InfiniteTimeSpan;
        }

        private void SetAuthorizationHeader()
        {
            var token = _tokenService.GetAccessToken();

            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                // Clear authorization if no token
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        public async Task<HttpResponseMessage?> GetAsync(string endpoint)
        {
            try
            {
                SetAuthorizationHeader();
                return await _httpClient.GetAsync(endpoint);
            }
            catch (HttpRequestException)
            {
                // API unreachable
                return null;
            }
            catch (TaskCanceledException)
            {
                // timeout
                return null;
            }
        }

        public async Task<HttpResponseMessage?> PostAsync<T>(string endpoint, T data)
        {
            try
            {
                SetAuthorizationHeader();

                var content = new StringContent(
                    System.Text.Json.JsonSerializer.Serialize(data, new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                    }),
                    System.Text.Encoding.UTF8,
                    "application/json");


                return await _httpClient.PostAsync(endpoint, content);
            }
            catch (HttpRequestException)
            {
                // API unreachable
                return null;
            }
            catch (TaskCanceledException)
            {
                // timeout
                return null;
            }
        }

        public async Task<HttpResponseMessage?> PutAsync<T>(string endpoint, T data)
        {
            try
            {
                SetAuthorizationHeader();

                var content = new StringContent(
                    System.Text.Json.JsonSerializer.Serialize(data),
                    System.Text.Encoding.UTF8,
                    "application/json");

                return await _httpClient.PutAsync(endpoint, content);
            }
            catch (HttpRequestException)
            {
                // API unreachable
                return null;
            }
            catch (TaskCanceledException)
            {
                // timeout
                return null;
            }
        }

        public async Task<HttpResponseMessage?> DeleteAsync(string endpoint)
        {
            try
            {
                SetAuthorizationHeader();
                return await _httpClient.DeleteAsync(endpoint);
            }
            catch (HttpRequestException)
            {
                // API unreachable
                return null;
            }
            catch (TaskCanceledException)
            {
                // timeout
                return null;
            }
        }

        // DELETE with a JSON request body for endpoints that expect body and URL
        public async Task<HttpResponseMessage?> DeleteWithBodyAsync<T>(string endpoint, T data)
        {
            try
            {
                SetAuthorizationHeader();
                var request = new HttpRequestMessage(HttpMethod.Delete, endpoint)
                {
                    Content = new StringContent(
                        System.Text.Json.JsonSerializer.Serialize(data, new System.Text.Json.JsonSerializerOptions
                        {
                            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                        }),
                        System.Text.Encoding.UTF8,
                        "application/json")
                };
                return await _httpClient.SendAsync(request);
            }
            catch (HttpRequestException) { return null; }
            catch (TaskCanceledException) { return null; }
        }
    }
}