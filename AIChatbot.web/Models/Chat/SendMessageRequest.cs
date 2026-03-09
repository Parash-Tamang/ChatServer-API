using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;

namespace AIChatbot.web.Models.Chat
{
    /// <summary>
    /// Request to send a chat message to the API
    /// Matches ChatRequest from backend
    /// If ChatSessionId is null, a new chat session will be created
    /// </summary>
    public class SendMessageRequest
    {
        [JsonPropertyName("chatSessionId")]
        public Guid? ChatSessionId { get; set; }

        [Required]
        [MinLength(1)]
        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }
}