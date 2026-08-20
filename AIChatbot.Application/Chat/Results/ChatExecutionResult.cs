using AIChatbot.Application.Common;

namespace AIChatbot.Application.Chat.Results;

public sealed class ChatExecutionResult
{
    public ExecutionStatus Status { get; init; }

    public Guid ChatSessionId { get; init; }

    public Guid MessageId { get; init; }

    public string? AssistantReply { get; init; }

    // =====================================
    // EXCEL
    // =====================================

    public bool ExcelGenerated { get; init; }

    public object? ExcelAvailableNow { get; init; }

    // =====================================
    // GRAPH
    // =====================================

    public string? GraphType { get; init; }

    public string? GraphTitle { get; init; }

    public string? GraphImageUrl { get; init; }
    public string? GraphImageBase64 { get; init; }
}