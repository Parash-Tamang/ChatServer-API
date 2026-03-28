using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Net.Http;

namespace AIChatbot.web.Models.Chat
{
    /// <summary>
    /// Represents the result of a chat operation (send message, retry, etc.)
    /// </summary>
    public class ChatExecutionResult
    {
        [JsonPropertyName("status")]
        public int Status { get; set; }

        [JsonPropertyName("chatSessionId")]
        public Guid? ChatSessionId { get; set; }

        [JsonPropertyName("messageId")]
        public Guid? MessageId { get; set; }

        [JsonPropertyName("assistantReply")]
        public string? AssistantReply { get; set; }

        [JsonPropertyName("columns")]
        public List<ColumnDefinition>? Columns { get; set; }

        [JsonPropertyName("rows")]
        public List<List<JsonElement>>? Rows { get; set; }
    }

    public class ColumnDefinition
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;
    }
}