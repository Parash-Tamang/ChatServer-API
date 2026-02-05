namespace AIChatbot.Application.DTOs;


/// DTO representing a single chat message

public class MessageDto
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
