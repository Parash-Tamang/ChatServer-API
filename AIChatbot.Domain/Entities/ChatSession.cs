namespace AIChatbot.Domain.Entities;

public class ChatSession
{
    public Guid Id { get; set; }

    public string UserId { get; set; } = default!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
