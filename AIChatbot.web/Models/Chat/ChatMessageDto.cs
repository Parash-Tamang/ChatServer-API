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

        [JsonPropertyName("excelGenerated")]
        public bool ExcelGenerated { get; set; }

        [JsonPropertyName("excelAvailableNow")]
        public ExcelAvailableNow? ExcelAvailableNow { get; set; }

        [JsonPropertyName("graphType")]
        public string? GraphType { get; set; }

        [JsonPropertyName("graphTitle")]
        public string? GraphTitle { get; set; }

        [JsonPropertyName("graphImageUrl")]
        public string? GraphImageUrl { get; set; }

        [JsonPropertyName("graphImageBase64")]
        public string? GraphImageBase64 { get; set; }
    }
}
