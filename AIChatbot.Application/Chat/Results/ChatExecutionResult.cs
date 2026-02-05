using AIChatbot.Application.Common;

namespace AIChatbot.Application.Chat.Results;

/// Result returned when sending a chat message

public sealed class ChatExecutionResult
{
    public ExecutionStatus Status { get; init; }
    public Guid ChatSessionId { get; init; }
    public string? AssistantReply { get; init; }
}
