using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;

namespace AIChatbot.web.Models.Chat
{
    /// <summary>
    /// Request to retry a chat message
    /// Used internally by frontend, sent to /Chat/Retry controller action
    /// </summary>
    public class RetryRequest
    {
        [JsonPropertyName("chatSessionId")]
        public Guid ChatSessionId { get; set; }

        [JsonPropertyName("messageId")]
        public Guid MessageId { get; set; }
    }
}
