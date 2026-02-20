namespace AIChatbot.web.Models.Chat
{
    public class ChatExecutionResult
    {
        public int Status { get; set; }
        public Guid ChatSessionId { get; set; }
        public string? AssistantReply { get; set; }
    }

}
