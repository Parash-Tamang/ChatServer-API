namespace AIChatbot.web.Dto
{
    public class AuthResponseDto
    {
        public bool Success { get; set; }

        public string? UserId { get; set; }

        public string? AccessToken { get; set; }

        public string? RefreshToken { get; set; }

        public int ExpiresIn { get; set; }

        public string? Error { get; set; }

        public List<string>? Roles { get; set; }
    }
}
