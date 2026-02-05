namespace AIChatbot.Domain.Entities;

/// <summary>
/// Represents a chat session owned by a user
/// </summary>
public class ChatSession
{
    public Guid Id { get; set; }

   
    /// Owner of the chat session
   
    public string UserId { get; set; } = default!;

    /// Session creation timestamp (UTC)
   
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

   
    /// Messages belonging to this session
   
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
