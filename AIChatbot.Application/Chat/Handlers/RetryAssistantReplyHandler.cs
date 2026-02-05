using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Chat.Commands;
using AIChatbot.Application.Chat.Results;
using AIChatbot.Application.Common;
using MediatR;

namespace AIChatbot.Application.Chat.Handlers;


/// Retries AI assistant reply for a specific user message

public class RetryAssistantReplyHandler
    : IRequestHandler<RetryAssistantReplyCommand, ChatCommandResult>
{
    private readonly IChatSessionRepository _repo;
    private readonly IAiProviderService _ai;

    public RetryAssistantReplyHandler(
        IChatSessionRepository repo,
        IAiProviderService ai)
    {
        _repo = repo;
        _ai = ai;
    }

    public async Task<ChatCommandResult> Handle(
        RetryAssistantReplyCommand request,
        CancellationToken cancellationToken)
    {
        // Security: session must belong to user
        var ownsSession = await _repo.ChatSessionBelongsToUser(
            request.ChatSessionId,
            request.UserId);

        if (!ownsSession)
            throw new UnauthorizedAccessException();

        // Load the user message
        var msg = await _repo.GetMessageAsync(request.MessageId);

        if (msg == null || msg.Role != "user")
            throw new InvalidOperationException("Only user messages can be retried.");

        try
        {
            // Generate reply using only this message
            var reply = await _ai.GetReplyAsync(new[] { msg });

            // ✅ FIX: correct repository call
            await _repo.SaveMessageAsync(
                msg.ChatSessionId,
                "assistant",
                reply);

            return new ChatCommandResult
            {
                Status = ExecutionStatus.Success,
                ChatSessionId = msg.ChatSessionId,
                AssistantReply = reply
            };
        }
        catch
        {
            return new ChatCommandResult
            {
                Status = ExecutionStatus.PartiallyExecuted,
                ChatSessionId = msg.ChatSessionId
            };
        }
    }
}
