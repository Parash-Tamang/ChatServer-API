namespace AIChatbot.web.Services
{
    public class ApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly TokenService _tokenService;

        public ApiClient(HttpClient httpClient, IConfiguration configuration, TokenService tokenService)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _tokenService = tokenService;
            
            // Set base address from configuration
            var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://192.168.0.104:5197";
            _httpClient.BaseAddress = new Uri(apiBaseUrl);
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

        public async Task<HttpResponseMessage> GetAsync(string endpoint)
        {
            SetAuthorizationHeader();
            return await _httpClient.GetAsync(endpoint);
        }

        public async Task<HttpResponseMessage> PostAsync<T>(string endpoint, T data)
        {
            SetAuthorizationHeader();

            var content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(data),
                System.Text.Encoding.UTF8,
                "application/json");

            return await _httpClient.PostAsync(endpoint, content);
        }

        public async Task<HttpResponseMessage> PutAsync<T>(string endpoint, T data)
        {
            SetAuthorizationHeader();

            var content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(data),
                System.Text.Encoding.UTF8,
                "application/json");

            return await _httpClient.PutAsync(endpoint, content);
        }

        public async Task<HttpResponseMessage> DeleteAsync(string endpoint)
        {
            SetAuthorizationHeader();
            return await _httpClient.DeleteAsync(endpoint);
        }
    }
}
