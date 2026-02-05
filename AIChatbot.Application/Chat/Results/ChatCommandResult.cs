using AIChatbot.Application.Common;

namespace AIChatbot.Application.Chat.Results;

/// Result returned from retry-based chat commands

public sealed class ChatCommandResult
{
    public ExecutionStatus Status { get; init; }
    public Guid ChatSessionId { get; init; }
    public string? AssistantReply { get; init; }
}
