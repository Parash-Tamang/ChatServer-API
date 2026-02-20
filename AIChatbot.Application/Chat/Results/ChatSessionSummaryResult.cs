namespace AIChatbot.Application.Chat.Results;

public sealed class ChatSessionSummaryResult
{
    public Guid Id { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? TopicName { get; init; }
}
