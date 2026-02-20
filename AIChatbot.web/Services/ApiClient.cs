using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AIChatbot.web.Models.Auth;

namespace AIChatbot.web.Services
{
    public class ApiClient
    {
        private readonly IHttpClientFactory _factory;
        private readonly TokenService _token;
        private readonly IConfiguration _config;

        public ApiClient(
            IHttpClientFactory factory,
            TokenService token,
            IConfiguration config)
        {
            _factory = factory;
            _token = token;
            _config = config;
        }

        private async Task<HttpClient> CreateClient()
        {
            var client = _factory.CreateClient();

            var baseUrl = _config["ApiSettings:BaseUrl"];
            if (string.IsNullOrEmpty(baseUrl))
                throw new Exception("ApiSettings:BaseUrl missing");

            client.BaseAddress = new Uri(baseUrl);

            var access = await _token.GetValidAccessToken();

            if (!string.IsNullOrEmpty(access))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", access);
            }

            return client;
        }

        public async Task<HttpResponseMessage> GetAsync(string url)
        {
            var client = await CreateClient();
            var response = await client.GetAsync(url);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                if (await TryRefreshToken())
                {
                    client = await CreateClient();
                    response = await client.GetAsync(url);
                }
            }

            return response;
        }

        public async Task<HttpResponseMessage> PostAsync(string url, object body)
        {
            var client = await CreateClient();
            var response = await client.PostAsJsonAsync(url, body);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                if (await TryRefreshToken())
                {
                    client = await CreateClient();
                    response = await client.PostAsJsonAsync(url, body);
                }
            }

            return response;
        }

        public async Task<HttpResponseMessage> DeleteAsync(string url)
        {
            var client = await CreateClient();
            var response = await client.DeleteAsync(url);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                if (await TryRefreshToken())
                {
                    client = await CreateClient();
                    response = await client.DeleteAsync(url);
                }
            }

            return response;
        }

        private async Task<bool> TryRefreshToken()
        {
            var refresh = _token.GetRefreshToken();
            if (string.IsNullOrEmpty(refresh))
                return false;

            var client = _factory.CreateClient();

            var baseUrl = _config["ApiSettings:BaseUrl"];

            if (string.IsNullOrEmpty(baseUrl))
                throw new Exception("BaseUrl missing in appsettings");

            client.BaseAddress = new Uri(baseUrl);


            var res = await client.PostAsJsonAsync(
                "/api/auth/V1/Security-engine/refresh",
                new { refreshToken = refresh });

            if (!res.IsSuccessStatusCode)
                return false;

            var data = await res.Content.ReadFromJsonAsync<AuthResponse>();

            if (data == null || !data.Success)
                return false;

            _token.SaveTokens(data.AccessToken!, data.RefreshToken!, 3600);

            return true;
        }
    }
}
