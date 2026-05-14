namespace AIChatbot.web.Dto
{
    public class VerifyRegisterOtpResponseDto
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public string? RegisterToken { get; set; }
    }
}
