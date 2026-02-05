namespace AIChatbot.Application.Chat.Results;


/// DTO representing a chat message

public sealed class ChatMessageResult
{
    public Guid Id { get; init; }
    public string Role { get; init; } = default!;
    public string Content { get; init; } = default!;
    public DateTime CreatedAt { get; init; }
}
