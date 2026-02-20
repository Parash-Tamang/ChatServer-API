namespace AIChatbot.web.Models.Chat
{
    public class RetryRequest
    {
        public Guid ChatSessionId { get; set; }
        public Guid MessageId { get; set; }
    }

}
