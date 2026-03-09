using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
namespace AIChatbot.web.Models.Chat
{
    /// <summary>
    /// Represents a chat session summary returned from API
    /// Matches ChatSessionSummaryResult from backend
    /// </summary>
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
