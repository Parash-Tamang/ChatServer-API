namespace AIChatbot.Domain.Entities;

/// Represents a single message in a chat session
public class Message
{
    public Guid Id { get; set; }

    public Guid ChatSessionId { get; set; }

    /// Message role: "user" or "assistant"
    public string Role { get; set; } = default!;

    public string Content { get; set; } = default!;

    /// Message creation timestamp (UTC)
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// Navigation property
    public ChatSession ChatSession { get; set; } = default!;
}
