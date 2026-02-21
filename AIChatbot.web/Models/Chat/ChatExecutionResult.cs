using System.Text.Json.Serialization;

namespace AIChatbot.web.Models.Chat
{
    /// <summary>
    /// Represents the result of a chat operation (send message, retry, etc.)
    /// Matches ChatExecutionResult from backend
    /// </summary>
    public class ChatExecutionResult
    {
        [JsonPropertyName("status")]
        public int Status { get; set; }

        [JsonPropertyName("chatSessionId")]
        public Guid ChatSessionId { get; set; }

        [JsonPropertyName("messageId")]
        public Guid MessageId { get; set; }

        [JsonPropertyName("assistantReply")]
        public string? AssistantReply { get; set; }
    }
}
