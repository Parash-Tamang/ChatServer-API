namespace AIChatbot.Application.DTOs;

public class ChatSessionDto
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<MessageDto> Messages { get; set; } = new();
}
