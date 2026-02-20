namespace AIChatbot.Domain.Entities;

public class ChatSessionNaming
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ChatSessionId { get; set; }
    public Guid MessageId { get; set; }

    public string TopicName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
