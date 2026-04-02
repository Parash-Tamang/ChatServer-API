namespace AIChatbot.web.Interfaces
{
   
        public interface ITokenService
        {
            void SaveTokens(string accessToken, string refreshToken, int expiresIn);

            string? GetAccessToken();

            string? GetRefreshToken();

            bool HasAccessToken();

            bool HasRefreshToken();

            bool IsAccessTokenExpired();

            bool IsAuthenticated();

            void ClearTokens();
        }
    }

