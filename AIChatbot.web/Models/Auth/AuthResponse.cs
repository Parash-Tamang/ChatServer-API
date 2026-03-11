namespace AIChatbot.web.Models.Auth
{
    public class AuthResponse
    {
        public bool Success { get; set; }
        public   string userId { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public int ExpiresIn { get; set; } = 3600; // Default 1 hour in seconds
        public string? Error { get; set; }
        // ✅ API returns array ["Admin"], not a string
        public List<string>? Roles { get; set; }
    }
  
}
