using System.Text.Json.Serialization;

namespace AIChatbot.web.Models.Chat
{
    public class ChatSessionDto
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("topicName")]
        public string? TopicName { get; set; }
    }
}
