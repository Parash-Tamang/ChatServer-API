using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
namespace AIChatbot.web.Models.Chat
{
    /// <summary>
    /// Represents a single chat message returned from API
    /// Matches MessageDto from backend
    /// </summary>
    public class ChatMessageDto
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("columns")]
        public List<ColumnDefinition>? Columns { get; set; }

        [JsonPropertyName("rows")]
        public List<List<JsonElement>>? Rows { get; set; }
    }
}
