using System.Text.Json.Serialization;

namespace AIChatbot.web.Models.Chat
{
    /// <summary>
    /// Represents a single chat message returned from API
    /// Matches MessageDto from backend
    /// </summary>
    public class ChatMessageDto
    {
        [JsonPropertyName("role")]
        public string? Role { get; set; }

        [JsonPropertyName("content")]
        public string? Content { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }
    }
}
