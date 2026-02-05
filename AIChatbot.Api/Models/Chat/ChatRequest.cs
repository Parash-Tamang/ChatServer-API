using System.ComponentModel.DataAnnotations;

namespace AIChatbot.Api.Models.Chat;


/// Request to send a chat message.
/// If ChatSessionId is null, a new chat session will be created.

public class ChatRequest
{
    
    /// Existing chat session identifier (optional).
    /// Null indicates a new chat session.
    
    public Guid? ChatSessionId { get; set; }


    /// User message to be processed by the AI assistant
    
    [Required]
    [MinLength(1)]
    public string Message { get; set; } = string.Empty;
}
