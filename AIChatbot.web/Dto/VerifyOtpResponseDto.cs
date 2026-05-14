namespace AIChatbot.web.Dto
{
    public class VerifyOtpResponseDto
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public string? Token { get; set; }
    }
}
