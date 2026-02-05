using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Chat.Commands;
using AIChatbot.Application.Chat.Results;
using AIChatbot.Application.Common;
using MediatR;

namespace AIChatbot.Application.Chat.Handlers;


/// Handles sending a user message and generating AI response

public class SendChatMessageHandler
    : IRequestHandler<SendChatMessageCommand, ChatExecutionResult>
{
    private readonly IChatSessionRepository _repo;
    private readonly IAiProviderService _ai;

    public SendChatMessageHandler(
        IChatSessionRepository repo,
        IAiProviderService ai)
    {
        _repo = repo;
        _ai = ai;
    }

    public async Task<ChatExecutionResult> Handle(
        SendChatMessageCommand request,
        CancellationToken cancellationToken)
    {
        // Create new session if none provided
        var sessionId = request.ChatSessionId
            ?? await _repo.CreateChatSessionAsync(request.UserId);

        // Persist user message first (authoritative)
        await _repo.SaveMessageAsync(
            sessionId,
            "user",
            request.Message);

        try
        {
            var messages = await _repo.GetMessagesAsync(sessionId);
            var reply = await _ai.GetReplyAsync(messages);

            await _repo.SaveMessageAsync(
                sessionId,
                "assistant",
                reply);

            return new ChatExecutionResult
            {
                Status = ExecutionStatus.Success,
                ChatSessionId = sessionId,
                AssistantReply = reply
            };
        }
        catch
        {
            return new ChatExecutionResult
            {
                Status = ExecutionStatus.PartiallyExecuted,
                ChatSessionId = sessionId
            };
        }
    }
}
