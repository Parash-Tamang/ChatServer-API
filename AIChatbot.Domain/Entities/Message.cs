namespace AIChatbot.Domain.Entities;
public class Message
{
    public Guid Id { get; set; }

    public Guid ChatSessionId { get; set; }

    public string Role { get; set; } = default!;
    public string Content { get; set; } = default!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ChatSession ChatSession { get; set; } = default!;
}
