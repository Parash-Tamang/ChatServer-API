using AIChatbot.Application.Common;

namespace AIChatbot.Application.Chat.Results;

public sealed class ChatExecutionResult
{
    public ExecutionStatus Status { get; init; }
    public Guid ChatSessionId { get; init; }
    public Guid MessageId { get; init; }        // 🔥 ADD THIS
    public string? AssistantReply { get; init; }

    // Return table data from LLM when available
    public object? Columns { get; init; }
    public object? Rows { get; init; }
}
