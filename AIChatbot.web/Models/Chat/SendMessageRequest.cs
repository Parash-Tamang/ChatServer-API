namespace AIChatbot.web.Models.Chat
{
    public class SendMessageRequest
    {
        public Guid? ChatSessionId { get; set; }
        public string? Message { get; set; }
    }

}