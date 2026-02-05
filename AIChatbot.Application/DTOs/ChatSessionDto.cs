namespace AIChatbot.Application.DTOs;


/// DTO representing a chat session with its messages

public class ChatSessionDto
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }

    
    /// Messages belonging to this chat session
   
    public List<MessageDto> Messages { get; set; } = new();
}
